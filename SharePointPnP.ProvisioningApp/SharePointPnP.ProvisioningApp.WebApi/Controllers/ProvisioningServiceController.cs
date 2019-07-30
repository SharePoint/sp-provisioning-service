//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using SharePointPnP.ProvisioningApp.DomainModel;
using SharePointPnP.ProvisioningApp.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity;
using Newtonsoft.Json;
using SharePointPnP.ProvisioningApp.WebApi.Components;
using System.Configuration;
using SharePointPnP.ProvisioningApp.Infrastructure;
using SharePointPnP.ProvisioningApp.Infrastructure.DomainModel.Provisioning;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage.Queue;
using OfficeDevPnP.Core.Framework.Provisioning.CanProvisionRules;
using TenantAdmin = Microsoft.Online.SharePoint.TenantAdministration;
using OfficeDevPnP.Core.Framework.Provisioning.Model;
using OfficeDevPnP.Core.Framework.Provisioning.Providers.Xml;
using System.IO;
using System.Xml.Linq;
using OfficeDevPnP.Core.Framework.Provisioning.Providers;
using OfficeDevPnP.Core.Framework.Provisioning.Connectors;
using OfficeDevPnP.Core;
using OfficeDevPnP.Core.Framework.Provisioning.ObjectHandlers;

namespace SharePointPnP.ProvisioningApp.WebApi.Controllers
{
    /// <summary>
    /// Main API controller for APIs related to the provisioning of packages
    /// </summary>
    [Authorize]
    public class ProvisioningServiceController : ApiController
    {
        readonly ProvisioningAppDBContext dbContext = new ProvisioningAppDBContext();

        /// <summary>
        /// Returns the parameters for a package
        /// </summary>
        /// <param name="id">The ID of the target package</param>
        /// <returns>A collection of parameters for the package</returns>
        [Route("~/ProvisioningService/GetPackageParameters")]
        public async Task<List<PackageParameter>> GetPackageParameters(Guid id)
        {
            // Manage Authorization Checks
            ApiSecurityHelper.CheckRequestAuthorization(true);

            // Prepare the output object
            var result = new List<PackageParameter>();

            // Retrieve the package from the database
            var package = await dbContext.Packages.FirstOrDefaultAsync(p => p.Id == id);

            // If we found the package
            if (package != null)
            {
                // Deserialize the properties
                var properties = JsonConvert.DeserializeObject<Dictionary<String, String>>(package.PackageProperties);

                // Configure the metadata properties template
                var metadataTemplate = new
                {
                    properties = new[] {
                                    new {
                                        name = "",
                                        caption = "",
                                        description = "",
                                        editor = "",
                                        editorSettings = "",
                                    }
                                }
                };

                // And deserialize and process the metadata of properties
                var metadataProperties = JsonConvert.DeserializeAnonymousType(package.PropertiesMetadata, metadataTemplate);
                var metadata = metadataProperties.properties.ToDictionary(i => i.name);

                // Process every property
                foreach (var p in properties)
                {
                    // Get the mapping metadata, if any
                    var m = metadata.ContainsKey(p.Key) ? metadata[p.Key] : null;

                    // Build a single item for the output
                    result.Add(new PackageParameter
                    {
                        Name = p.Key,
                        DefaultValue = p.Value,
                        Caption =  m?.caption,
                        Description = m?.description,
                        Editor = m?.editor,
                        EditorSettings = m?.editorSettings
                    });
                }
            }

            // Return the generated output
            return (result);
        }

        /// <summary>
        /// Provisions a content pack
        /// </summary>
        /// <remarks>
        /// This operation was specifically designed for ODP integration
        /// </remarks>
        /// <param name="provisionRequest">The request to provision a content pack</param>
        /// <returns>The outcome of the provisioning request</returns>
        [HttpPost]
        [Route("~/ProvisioningService/ProvisionContentPack")]
        [AllowAnonymous]
        public async Task<ProvisionContentPackResponse> ProvisionContentPack(ProvisionContentPackRequest provisionRequest)
        {
            var provisionResponse = new ProvisionContentPackResponse();

            // If the input paramenters are missing, raise a BadRequest response
            if (provisionRequest == null)
            {
                ThrowEmptyRequest();
            }

            // If the TenantId input argument is missing, raise a BadRequest response
            if (String.IsNullOrEmpty(provisionRequest.TenantId))
            {
                ThrowMissingArgument("TenantId");
            }

            // If the UserPrincipalName input argument is missing, raise a BadRequest response
            if (String.IsNullOrEmpty(provisionRequest.UserPrincipalName))
            {
                ThrowMissingArgument("UserPrincipalName");
            }

            // If the PackageIds input argument is missing, raise a BadRequest response
            if (provisionRequest.Packages == null || provisionRequest.Packages.Count == 0)
            {
                ThrowMissingArgument("Packages");
            }

            if (provisionRequest != null &&
                !String.IsNullOrEmpty(provisionRequest.TenantId) &&
                !String.IsNullOrEmpty(provisionRequest.UserPrincipalName) &&
                provisionRequest.Packages != null &&
                provisionRequest.Packages.Count > 0)
            {
                try
                {
                    // Process the AuthorizationCode request
                    String provisioningScope = ConfigurationManager.AppSettings["SPPA:ProvisioningScope"];
                    String provisioningEnvironment = ConfigurationManager.AppSettings["SPPA:ProvisioningEnvironment"];

                    var tokenId = $"{provisionRequest.TenantId}-{provisionRequest.UserPrincipalName.GetHashCode()}-{provisioningScope}-{provisioningEnvironment}";

                    try
                    {
                        // Retrieve the refresh token and store it in the KeyVault
                        await ProvisioningAppManager.AccessTokenProvider.SetupSecurityFromAuthorizationCodeAsync(
                            tokenId,
                            provisionRequest.AuthorizationCode,
                            provisionRequest.TenantId,
                            ConfigurationManager.AppSettings["ida:ClientId"],
                            ConfigurationManager.AppSettings["ida:ClientSecret"],
                            "https://graph.microsoft.com/",
                            provisionRequest.RedirectUri);
                    }
                    catch (Exception ex)
                    {
                        // In case of any authorization exception, raise an Unauthorized exception
                        ThrowUnauthorized(ex);
                    }

                    // Validate the Package IDs
                    var context = dbContext;
                    DomainModel.Package package = null;

                    // Get the first item to provision
                    var item = provisionRequest.Packages.First();

                    // And remove it from the whole list of items
                    provisionRequest.Packages.RemoveAt(0);
                    var childrenItems = provisionRequest.Packages;

                    // Get the package
                    if (ProvisioningAppManager.IsTestingEnvironment)
                    {
                        // Process all packages in the test environment
                        package = context.Packages.FirstOrDefault(p => p.Id == new Guid(item.PackageId));
                    }
                    else
                    {
                        // Process not-preview packages in the production environment
                        package = context.Packages.FirstOrDefault(p => p.Id == new Guid(item.PackageId) && p.Preview == false);
                    }

                    // If the package is not valid
                    if (package == null)
                    {
                        // Throw an exception accordingly
                        throw new ArgumentException("Invalid Package Id!");
                    }

                    // First of all, validate the provisioning request for pre-requirements
                    provisionResponse.CanProvisionResult = await CanProvisionInternal(
                        new CanProvisionModel
                        {
                            PackageId = item.PackageId,
                            TenantId = provisionRequest.TenantId,
                            UserPrincipalName = provisionRequest.UserPrincipalName,
                            SPORootSiteUrl = provisionRequest.SPORootSiteUrl,
                            UserIsSPOAdmin = true, // We assume that the request comes from an Admin
                            UserIsTenantAdmin = true, // We assume that the request comes from an Admin
                        });

                    // If the package can be provisioned onto the target
                    if (provisionResponse.CanProvisionResult.CanProvision)
                    {
                        // Prepare the provisioning request
                        var request = new ProvisioningActionModel();
                        request.ActionType = ActionType.Tenant; // Do we want to support site/tenant or just one?
                        request.ApplyCustomTheme = false;
                        request.ApplyTheme = false; // Do we need to apply any special theme?
                        request.CorrelationId = Guid.NewGuid();
                        request.CustomLogo = null;
                        request.DisplayName = $"Provision Content Pack {item.PackageId}";
                        request.PackageId = item.PackageId;
                        request.TargetSiteAlreadyExists = false; // Do we want to check this?
                        request.TargetSiteBaseTemplateId = null;
                        request.TenantId = provisionRequest.TenantId;
                        request.UserIsSPOAdmin = true; // We don't use this in the job
                        request.UserIsTenantAdmin = true; // We don't use this in the job
                        request.UserPrincipalName = provisionRequest.UserPrincipalName.ToLower();
                        request.NotificationEmail = provisionRequest.UserPrincipalName.ToLower();
                        request.PackageProperties = item.Parameters;
                        request.ChildrenItems = childrenItems;
                        request.Webhooks = provisionRequest.Webhooks;

                        // Enqueue the provisioning request
                        await ProvisioningAppManager.EnqueueProvisioningRequest(request);
                    }

                    // Set the status of the provisioning request
                    provisionResponse.ProvisioningStarted = true;
                }
                catch (HttpResponseException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // In case of any other exception, raise an InternalServerError exception
                    ThrowInternalServerError(ex);
                }
            }

            // Return to the requested URL
            return (provisionResponse);
        }

        /// <summary>
        /// Throws a BadRequest exception for an empty request
        /// </summary>
        private void ThrowEmptyRequest()
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
            {
                Content = new StringContent("The request is empty. Please provide proper JSON content for the request!"),
            };

            throw new HttpResponseException(response);
        }

        /// <summary>
        /// Throws a BadRequest exception for a missing input argument
        /// </summary>
        /// <param name="argument">The name of the missing argument</param>
        private void ThrowMissingArgument(String argument)
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
            {
                Content = new StringContent($"Missing {argument} argument in the request!"),
            };

            throw new HttpResponseException(response);
        }

        /// <summary>
        /// Throws an Unauthorized exception
        /// </summary>
        /// <param name="ex">The exception that occurred</param>
        private void ThrowUnauthorized(Exception ex)
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
            {
                Content = new StringContent($"Unauthorized request! {ex.Message}"),
            };

            throw new HttpResponseException(response);
        }

        /// <summary>
        /// Throws an Unauthorized exception
        /// </summary>
        /// <param name="ex">The exception that occurred</param>
        private void ThrowInternalServerError(Exception ex)
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(ex.Message),
            };

            throw new HttpResponseException(response);
        }

        private async Task<CanProvisionResult> CanProvisionInternal(CanProvisionModel model)
        {
            var canProvisionResult = new CanProvisionResult();

            String provisioningScope = ConfigurationManager.AppSettings["SPPA:ProvisioningScope"];
            String provisioningEnvironment = ConfigurationManager.AppSettings["SPPA:ProvisioningEnvironment"];

            var tokenId = $"{model.TenantId}-{model.UserPrincipalName.GetHashCode()}-{provisioningScope}-{provisioningEnvironment}";
            var graphAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                tokenId, "https://graph.microsoft.com/");

            // Retrieve the provisioning package from the database and from the Blob Storage
            var context = dbContext;
            DomainModel.Package package = null;

            // Get the package
            if (ProvisioningAppManager.IsTestingEnvironment)
            {
                // Process all packages in the test environment
                package = context.Packages.FirstOrDefault(p => p.Id == new Guid(model.PackageId));
            }
            else
            {
                // Process not-preview packages in the production environment
                package = context.Packages.FirstOrDefault(p => p.Id == new Guid(model.PackageId) && p.Preview == false);
            }

            if (package != null)
            {
                // Retrieve parameters from the package/template definition
                var packageFileUrl = new Uri(package.PackageUrl);
                var packageLocalFolder = packageFileUrl.AbsolutePath.Substring(1,
                    packageFileUrl.AbsolutePath.LastIndexOf('/') - 1);
                var packageFileName = packageFileUrl.AbsolutePath.Substring(packageLocalFolder.Length + 2);

                ProvisioningHierarchy hierarchy = GetHierarchyFromStorage(packageLocalFolder, packageFileName);

                // If we have the hierarchy
                if (hierarchy != null)
                {
                    var accessTokens = new Dictionary<String, String>();

                    AuthenticationManager authManager = new AuthenticationManager();
                    var ptai = new ProvisioningTemplateApplyingInformation();

                    // Retrieve the SPO URL for the Admin Site
                    var rootSiteUrl = model.SPORootSiteUrl;

                    // Retrieve the SPO Access Token for SPO
                    var spoAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                        tokenId, rootSiteUrl,
                        ConfigurationManager.AppSettings["ida:ClientId"],
                        ConfigurationManager.AppSettings["ida:ClientSecret"],
                        ConfigurationManager.AppSettings["ida:AppUrl"]);

                    // Store the SPO Access Token for any further context cloning
                    accessTokens.Add(new Uri(rootSiteUrl).Authority, spoAccessToken);

                    // Define a PnPProvisioningContext scope to share the security context across calls
                    using (var pnpProvisioningContext = new PnPProvisioningContext(async (r, s) =>
                    {
                        if (accessTokens.ContainsKey(r))
                        {
                            // In this scenario we just use the dictionary of access tokens
                            // in fact the overall operation for sure will take less than 1 hour
                            // (in fact, it's a matter of few seconds)
                            return await Task.FromResult(accessTokens[r]);
                        }
                        else
                        {
                            // Try to get a fresh new Access Token
                            var token = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                                tokenId, $"https://{r}",
                                ConfigurationManager.AppSettings["ida:ClientId"],
                                ConfigurationManager.AppSettings["ida:ClientSecret"],
                                ConfigurationManager.AppSettings["ida:AppUrl"]);

                            accessTokens.Add(r, token);

                            return (token);
                        }
                    }))
                    {
                        // If the user is an admin (SPO or Tenant) we run the Tenant level CanProvision rules
                        if (model.UserIsSPOAdmin || model.UserIsTenantAdmin)
                        {
                            // Retrieve the SPO URL for the Admin Site
                            var adminSiteUrl = model.SPORootSiteUrl.Replace(".sharepoint.com", "-admin.sharepoint.com");

                            // Retrieve the SPO Access Token for the Admin Site
                            var spoAdminAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                                tokenId, adminSiteUrl,
                                ConfigurationManager.AppSettings["ida:ClientId"],
                                ConfigurationManager.AppSettings["ida:ClientSecret"],
                                ConfigurationManager.AppSettings["ida:AppUrl"]);

                            // Store the SPO Admin Access Token for any further context cloning
                            accessTokens.Add(new Uri(adminSiteUrl).Authority, spoAdminAccessToken);

                            // Connect to SPO Admin Site and evaluate the CanProvision rules for the hierarchy
                            using (var tenantContext = authManager.GetAzureADAccessTokenAuthenticatedContext(adminSiteUrl, spoAdminAccessToken))
                            {
                                using (var pnpTenantContext = PnPClientContext.ConvertFrom(tenantContext))
                                {
                                    // Creat the Tenant object for the current SPO Admin Site context
                                    TenantAdmin.Tenant tenant = new TenantAdmin.Tenant(pnpTenantContext);

                                    // Run the CanProvision rules against the current tenant
                                    canProvisionResult = CanProvisionRulesManager.CanProvision(tenant, hierarchy, null, ptai);
                                }
                            }
                        }
                        else
                        {
                            // Otherwise we run the Site level CanProvision rules

                            // Connect to SPO Root Site and evaluate the CanProvision rules for the hierarchy
                            using (var clientContext = authManager.GetAzureADAccessTokenAuthenticatedContext(rootSiteUrl, spoAccessToken))
                            {
                                using (var pnpContext = PnPClientContext.ConvertFrom(clientContext))
                                {
                                    // Run the CanProvision rules against the root site
                                    canProvisionResult = CanProvisionRulesManager.CanProvision(pnpContext.Web, hierarchy.Templates[0], ptai);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                throw new ApplicationException("Invalid request, the requested package/template is not available!");
            }

            return canProvisionResult;
        }

        private static ProvisioningHierarchy GetHierarchyFromStorage(String packageLocalFolder, String packageFileName)
        {
            // Prepare the resulting value
            ProvisioningHierarchy hierarchy = null;

            // Create the template provider instance targeting the Blob Storage
            var provider = new XMLAzureStorageTemplateProvider(
                ConfigurationManager.AppSettings["BlobTemplatesProvider:ConnectionString"],
                packageLocalFolder);

            using (Stream stream = provider.Connector.GetFileStream(packageFileName))
            {
                // Crate a copy of the source stream
                MemoryStream mem = new MemoryStream();
                stream.CopyTo(mem);
                mem.Position = 0;

                if (packageFileName.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase))
                {
                    // That's an XML Provisioning Template file

                    XDocument xml = XDocument.Load(mem);
                    mem.Position = 0;

                    var formatter = XMLPnPSchemaFormatter.GetSpecificFormatter(xml.Root.Name.NamespaceName);
                    formatter.Initialize(provider);

                    hierarchy = ((IProvisioningHierarchyFormatter)formatter).ToProvisioningHierarchy(mem);
                }
                else if (packageFileName.EndsWith(".pnp", StringComparison.InvariantCultureIgnoreCase))
                {
                    // That's a PnP Package file

                    // Get a provider based on the in-memory .PNP Open XML file
                    OpenXMLConnector openXmlConnector = new OpenXMLConnector(mem);
                    XMLTemplateProvider openXmlProvider = new XMLOpenXMLTemplateProvider(
                        openXmlConnector);

                    // Get the .xml provisioning template file name
                    var xmlTemplateFileName = openXmlConnector.Info?.Properties?.TemplateFileName ??
                        packageFileName.Substring(packageFileName.LastIndexOf('/') + 1)
                        .ToLower().Replace(".pnp", ".xml");


                    // Get the full hierarchy
                    hierarchy = openXmlProvider.GetHierarchy(xmlTemplateFileName);
                }
            }

            return (hierarchy);
        }

        protected override void Dispose(bool disposing)
        {
            dbContext.Dispose();
            base.Dispose(disposing);
        }
    }
}