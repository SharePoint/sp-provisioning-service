//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.Azure;
using Microsoft.Owin.Security.Cookies;
using Microsoft.SharePoint.Client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using OfficeDevPnP.Core;
using OfficeDevPnP.Core.Framework.Provisioning.CanProvisionRules;
using OfficeDevPnP.Core.Framework.Provisioning.Connectors;
using OfficeDevPnP.Core.Framework.Provisioning.Model;
using OfficeDevPnP.Core.Framework.Provisioning.ObjectHandlers;
using OfficeDevPnP.Core.Framework.Provisioning.Providers;
using OfficeDevPnP.Core.Framework.Provisioning.Providers.Xml;
using SharePointPnP.ProvisioningApp.DomainModel;
using SharePointPnP.ProvisioningApp.Infrastructure;
using SharePointPnP.ProvisioningApp.Infrastructure.DomainModel.Provisioning;
using SharePointPnP.ProvisioningApp.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Xml.Serialization;
using TenantAdmin = Microsoft.Online.SharePoint.TenantAdministration;


namespace SharePointPnP.ProvisioningApp.WebApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        /// <summary>
        /// Logins the user and redirects to the returnUrl, if any
        /// </summary>
        /// <param name="returnUrl">The optional return URL after login</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Login(String returnUrl = null)
        {
            return Redirect(returnUrl ?? "/");
        }

        [HttpGet]
        public ActionResult Logout()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(HttpContext.GetOwinContext()
                           .Authentication.GetAuthenticationTypes()
                           .Select(o => o.AuthenticationType).ToArray());

            return Redirect("/");
        }

        public ActionResult Index()
        {
            return Redirect("/");
        }

        [AllowAnonymous]
        public ActionResult Error(Exception exception)
        {
            CheckBetaFlag();

            HandleErrorInfo model = null;
            if (exception != null)
            {
                model = new HandleErrorInfo(exception, "unknown", "unknown");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Provision(String packageId = null, String returnUrl = null)
        {
            if (String.IsNullOrEmpty(packageId))
            {
                throw new ArgumentNullException("packageId");
            }

            CheckBetaFlag();
            PrepareHeaderData(returnUrl);

            ProvisioningActionModel model = new ProvisioningActionModel();

            try
            {
                if (IsValidUser())
                {
                    var issuer = (System.Threading.Thread.CurrentPrincipal as System.Security.Claims.ClaimsPrincipal)?.FindFirst("iss");
                    if (issuer != null && !String.IsNullOrEmpty(issuer.Value))
                    {
                        var issuerValue = issuer.Value.Substring(0, issuer.Value.Length - 1);
                        var tenantId = issuerValue.Substring(issuerValue.LastIndexOf("/") + 1);
                        var upn = (System.Threading.Thread.CurrentPrincipal as System.Security.Claims.ClaimsPrincipal)?.FindFirst(ClaimTypes.Upn)?.Value;

                        if (this.IsAllowedUpnTenant(upn))
                        {
                            #region Prepare model generic context data

                            // Prepare the model data
                            model.TenantId = tenantId;
                            model.UserPrincipalName = upn;
                            model.PackageId = packageId;
                            model.ApplyTheme = false;
                            model.ApplyCustomTheme = false;

                            String provisioningScope = ConfigurationManager.AppSettings["SPPA:ProvisioningScope"];
                            String provisioningEnvironment = ConfigurationManager.AppSettings["SPPA:ProvisioningEnvironment"];

                            var tokenId = $"{model.TenantId}-{model.UserPrincipalName.ToLower().GetHashCode()}-{provisioningScope}-{provisioningEnvironment}";
                            var graphAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                                tokenId, "https://graph.microsoft.com/");
                            if (string.IsNullOrEmpty(graphAccessToken))
                            {
                                throw new ApplicationException($"Cannot retrieve a valid Access Token for user {model.UserPrincipalName.ToLower()} in tenant {model.TenantId}");
                            }

                            model.UserIsTenantAdmin = Utilities.UserIsTenantGlobalAdmin(graphAccessToken);
                            model.UserIsSPOAdmin = Utilities.UserIsSPOAdmin(graphAccessToken);
                            model.NotificationEmail = upn;

                            model.ReturnUrl = returnUrl;

                            #endregion

                            // Determine the URL of the root SPO site for the current tenant
                            var rootSiteJson = HttpHelper.MakeGetRequestForString("https://graph.microsoft.com/v1.0/sites/root", graphAccessToken);
                            SharePointSite rootSite = JsonConvert.DeserializeObject<SharePointSite>(rootSiteJson);

                            // Store the SPO Root Site URL in the Model
                            model.SPORootSiteUrl = rootSite.WebUrl;

                            // If the current user is an admin, we can get the available Themes
                            if (model.UserIsTenantAdmin || model.UserIsSPOAdmin)
                            {
                                await LoadThemesFromTenant(model, tokenId, rootSite, graphAccessToken);
                            }

                            LoadPackageDataIntoModel(packageId, model);

                            if (model.ProvisioningPreRequirements != null)
                            {
                                await CheckPreRequirements(model, tokenId);
                            }
                        }
                        else
                        {
                            throw new ApplicationException("Invalid request, the current tenant is not allowed to use this solution!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return View("Provision", model);
        }

        [HttpPost]
        public async Task<ActionResult> Provision(ProvisioningActionModel model)
        {
            CheckBetaFlag();
            PrepareHeaderData(model.ReturnUrl);

            if (model != null && ModelState.IsValid)
            {
                // Enqueue the provisioning request
                await ProvisioningAppManager.EnqueueProvisioningRequest(
                    model,
                    (Request.Files != null && Request.Files.Count > 0 && Request.Files[0].ContentLength > 0) ? Request.Files[0].FileName : null,
                    (Request.Files != null && Request.Files.Count > 0 && Request.Files[0].ContentLength > 0) ? Request.Files[0].InputStream : null
                    );

                // Get the service description content
                var context = GetDataContext();
                var contentPage = context.ContentPages.FirstOrDefault(cp => cp.Id == "system/pages/ProvisioningScheduled.md");

                if (contentPage != null)
                {
                    model.ProvisionDescription = contentPage.Content;
                }
            }

            return View("ProvisionQueued", model);
        }

        [HttpGet]
        public async Task<JsonResult> UrlIsAvailableInSPO(String url)
        {
            bool siteUrlInUse = false;
            string baseTemplateId = null;

            if (IsValidUser())
            {
                var issuer = (System.Threading.Thread.CurrentPrincipal as System.Security.Claims.ClaimsPrincipal)?.FindFirst("iss");
                if (issuer != null && !String.IsNullOrEmpty(issuer.Value))
                {
                    var issuerValue = issuer.Value.Substring(0, issuer.Value.Length - 1);
                    var tenantId = issuerValue.Substring(issuerValue.LastIndexOf("/") + 1);
                    var upn = (System.Threading.Thread.CurrentPrincipal as System.Security.Claims.ClaimsPrincipal)?.FindFirst(ClaimTypes.Upn)?.Value;

                    String provisioningScope = ConfigurationManager.AppSettings["SPPA:ProvisioningScope"];
                    String provisioningEnvironment = ConfigurationManager.AppSettings["SPPA:ProvisioningEnvironment"];

                    var tokenId = $"{tenantId}-{upn.ToLower().GetHashCode()}-{provisioningScope}-{provisioningEnvironment}";
                    var graphAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                        tokenId, "https://graph.microsoft.com/");

                    // Determine the URL of the root SPO site for the current tenant
                    var rootSiteJson = HttpHelper.MakeGetRequestForString("https://graph.microsoft.com/v1.0/sites/root", graphAccessToken);
                    SharePointSite rootSite = JsonConvert.DeserializeObject<SharePointSite>(rootSiteJson);

                    var rootSiteUrl = rootSite.WebUrl;

                    // Retrieve the SPO Access Token
                    var spoAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                        tokenId, rootSiteUrl,
                        ConfigurationManager.AppSettings["ida:ClientId"],
                        ConfigurationManager.AppSettings["ida:ClientSecret"],
                        ConfigurationManager.AppSettings["ida:AppUrl"]);

                    // Connect to SPO and check if the URL is available or not
                    AuthenticationManager authManager = new AuthenticationManager();
                    using (ClientContext spoContext = authManager.GetAzureADAccessTokenAuthenticatedContext(rootSiteUrl, spoAccessToken))
                    {
                        var targetUrl = $"{rootSiteUrl.TrimEnd(new char[] { '/' })}{url}";
                        siteUrlInUse = spoContext.WebExistsFullUrl(targetUrl);
                        if (siteUrlInUse)
                        {
                            using (ClientContext spoTargetSiteContext = authManager.GetAzureADAccessTokenAuthenticatedContext(targetUrl, spoAccessToken))
                            {
                                baseTemplateId = spoTargetSiteContext.Web.GetBaseTemplateId();
                            }
                        }
                    }
                }
            }

            return (Json(new { result = !siteUrlInUse, baseTemplateId }, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet));
        }

        [HttpPost]
        public async Task<JsonResult> CanProvisionPackage(CanProvisionModel model)
        {
            if (String.IsNullOrEmpty(model.PackageId))
            {
                throw new ArgumentNullException("PackageId");
            }

            if (String.IsNullOrEmpty(model.TenantId))
            {
                throw new ArgumentNullException("TenantId");
            }

            if (String.IsNullOrEmpty(model.UserPrincipalName))
            {
                throw new ArgumentNullException("UserPrincipalName");
            }

            CanProvisionResult canProvisionResult = await CanProvisionInternal(model);

            return Json(canProvisionResult);
        }

        [HttpGet]
        public JsonResult IsProvisioningCompleted(String correlationId)
        {
            bool running = false;
            bool failed = false;

            if (IsValidUser())
            {
                // Get the DB context
                var context = GetDataContext();

                // Check if there is the action item for the current action
                var targetId = Guid.Parse(correlationId);
                var existingItem = context.ProvisioningActionItems.FirstOrDefault(i => i.Id == targetId);

                // And in case it does exist return that the action is still running
                running = (existingItem != null) && (existingItem.FailedOn == null || !existingItem.FailedOn.HasValue);
                failed = (existingItem != null) && (existingItem.FailedOn != null && existingItem.FailedOn.HasValue);
            }

            return (Json(new { running, failed }, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet));
        }

        [HttpPost]
        public ActionResult CategoriesMenu(String returnUrl = null)
        {
            CategoriesMenuViewModel model = new CategoriesMenuViewModel();

            // Let's see if we need to filter the output categories
            var slbHost = System.Configuration.ConfigurationManager.AppSettings["SPLBSiteHost"];
            var testEnvironment = Boolean.Parse(ConfigurationManager.AppSettings["TestEnvironment"]);

            string targetPlatform = null;

            if (!String.IsNullOrEmpty(returnUrl) &&
                !String.IsNullOrEmpty(slbHost) &&
                returnUrl.Contains(slbHost))
            {
                model.BaseLinksUrl = returnUrl.Substring(0, returnUrl.LastIndexOf(@"/") + 1);
                targetPlatform = "LOOKBOOK";
            }
            else
            {
                model.BaseLinksUrl = String.Empty;
                targetPlatform = "SPPNP";
            }

            // Get all the Categories together with the Packages
            ProvisioningAppDBContext context = new ProvisioningAppDBContext();

            var tempCategories = context.Categories
                .AsNoTracking()
                .Where(c => c.Packages.Any(
                    p => p.Visible &&
                    (testEnvironment || !p.Preview) &&
                    p.TargetPlatforms.Any(pf => pf.Id == targetPlatform)
                ))
                .OrderBy(c => c.Order)
                .Include("Packages")
                .ToList();

            model.Categories = tempCategories;

            return PartialView("CategoriesMenu", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ProvisionContentPack(ProvisionContentPackRequest provisionRequest)
        {
            var provisionResponse = new ProvisionContentPackResponse();

            // If the input paramenters are missing, raise a BadRequest response
            if (provisionRequest == null)
            {
                return ThrowEmptyRequest();
            }

            // If the TenantId input argument is missing, raise a BadRequest response
            if (String.IsNullOrEmpty(provisionRequest.TenantId))
            {
                return ThrowMissingArgument("TenantId");
            }

            // If the UserPrincipalName input argument is missing, raise a BadRequest response
            if (String.IsNullOrEmpty(provisionRequest.UserPrincipalName))
            {
                return ThrowMissingArgument("UserPrincipalName");
            }

            // If the PackageId input argument is missing, raise a BadRequest response
            if (String.IsNullOrEmpty(provisionRequest.PackageId))
            {
                return ThrowMissingArgument("PackageId");
            }

            if (provisionRequest != null &&
                !String.IsNullOrEmpty(provisionRequest.TenantId) &&
                !String.IsNullOrEmpty(provisionRequest.UserPrincipalName) &&
                !String.IsNullOrEmpty(provisionRequest.PackageId))
            {
                try
                {
                    // Validate the Package ID
                    var context = GetDataContext();
                    DomainModel.Package package = null;

                    // Get the package
                    if (ProvisioningAppManager.IsTestingEnvironment)
                    {
                        // Process all packages in the test environment
                        package = context.Packages.FirstOrDefault(p => p.Id == new Guid(provisionRequest.PackageId));
                    }
                    else
                    {
                        // Process not-preview packages in the production environment
                        package = context.Packages.FirstOrDefault(p => p.Id == new Guid(provisionRequest.PackageId) && p.Preview == false);
                    }

                    // If the package is not valid
                    if (package == null)
                    {
                        // Throw an exception accordingly
                        throw new ArgumentException("Invalid Package Id!");
                    }

                    String provisioningScope = package.PackageType == PackageType.Tenant ? "tenant" : "site";
                    String provisioningEnvironment = ConfigurationManager.AppSettings["SPPA:ProvisioningEnvironment"];

                    var tokenId = $"{provisionRequest.TenantId}-{provisionRequest.UserPrincipalName.ToLower().GetHashCode()}-{provisioningScope}-{provisioningEnvironment}";

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
                        return ThrowUnauthorized(ex);
                    }

                    // First of all, validate the provisioning request for pre-requirements
                    provisionResponse.CanProvisionResult = await CanProvisionInternal(
                        new CanProvisionModel
                        {
                            PackageId = provisionRequest.PackageId,
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
                        request.DisplayName = "Provision Content Pack";
                        request.PackageId = provisionRequest.PackageId;
                        request.TargetSiteAlreadyExists = false; // Do we want to check this?
                        request.TargetSiteBaseTemplateId = null;
                        request.TenantId = provisionRequest.TenantId;
                        request.UserIsSPOAdmin = true; // We don't use this in the job
                        request.UserIsTenantAdmin = true; // We don't use this in the job
                        request.UserPrincipalName = provisionRequest.UserPrincipalName.ToLower();
                        request.NotificationEmail = provisionRequest.UserPrincipalName.ToLower();
                        request.PackageProperties = provisionRequest.Parameters;

                        // Enqueue the provisioning request
                        await ProvisioningAppManager.EnqueueProvisioningRequest(request);

                        // Set the status of the provisioning request
                        provisionResponse.ProvisioningStarted = true;
                    }
                }
                catch (Exception ex)
                {
                    // In case of any other exception, raise an InternalServerError exception
                    return ThrowInternalServerError(ex);
                }
            }

            // Return to the requested URL
            return Json(provisionResponse);
        }

        private void CheckBetaFlag()
        {
            var isBeta = ConfigurationManager.AppSettings["IsBeta"];
            ViewBag.IsBeta = (!String.IsNullOrEmpty(isBeta) && Boolean.Parse(isBeta));
        }

        private void PrepareHeaderData(String returnUrl)
        {
            var headerData = new SharePointPnP.ProvisioningApp.WebApp.Models.HeaderDataViewModel();
            var slbHost = System.Configuration.ConfigurationManager.AppSettings["SPLBSiteHost"];

            if (!String.IsNullOrEmpty(returnUrl) &&
                !String.IsNullOrEmpty(slbHost) &&
                returnUrl.Contains(slbHost))
            {
                headerData.SiteTitle = "SharePoint Look Book";
                headerData.RootSiteUrl = returnUrl;
                headerData.RenderProvisioningService = false;
            }
            else
            {
                headerData.SiteTitle = "SharePoint Provisioning Service";
                headerData.RootSiteUrl = "/";
                headerData.RenderProvisioningService = true;
            }

            ViewBag.HeaderData = headerData;
        }

        /// <summary>
        /// Throws a BadRequest exception for an empty request
        /// </summary>
        private HttpStatusCodeResult ThrowEmptyRequest()
        {
            return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest,
                "The request is empty. Please provide proper JSON content for the request!");
        }

        /// <summary>
        /// Throws a BadRequest exception for a missing input argument
        /// </summary>
        /// <param name="argument">The name of the missing argument</param>
        private HttpStatusCodeResult ThrowMissingArgument(String argument)
        {
            return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest,
                $"Missing {argument} argument in the request!");
        }

        /// <summary>
        /// Throws an Unauthorized exception
        /// </summary>
        /// <param name="ex">The exception that occurred</param>
        private HttpStatusCodeResult ThrowUnauthorized(Exception ex)
        {
            return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden,
                $"Unauthorized request! {ex.Message}");
        }

        /// <summary>
        /// Throws an Unauthorized exception
        /// </summary>
        /// <param name="ex">The exception that occurred</param>
        private HttpStatusCodeResult ThrowInternalServerError(Exception ex)
        {
            return new HttpStatusCodeResult(System.Net.HttpStatusCode.InternalServerError,
                ex.Message);
        }

        private bool IsAllowedUpnTenant(string upn)
        {
            if (ProvisioningAppManager.IsTestingEnvironment)
            {
                // In test we support white-listed tenants only
                var context = GetDataContext();

                if (context.Tenants.Count() == 0)
                {
                    // If the tenants list does not exist
                    // all tenants are allowed
                    return (true);
                }
                else
                {
                    // Search for a matching tenant
                    var tenantName = upn.Substring(upn.IndexOf('@') + 1);
                    var matchingTenant = context.Tenants.FirstOrDefault(t => t.TenantName == tenantName);

                    // If we have a matching tenant
                    return (matchingTenant != null);
                }
            }
            else
            {
                // In production we support all the tenants
                return (true);
            }
        }

        private ProvisioningAppDBContext GetDataContext()
        {
            var context = new ProvisioningAppDBContext();
            context.Configuration.ProxyCreationEnabled = false;
            context.Configuration.LazyLoadingEnabled = false;
            context.Configuration.AutoDetectChangesEnabled = false;

            return context;
        }

        private static bool IsValidUser()
        {
            return System.Threading.Thread.CurrentPrincipal != null &&
                            System.Threading.Thread.CurrentPrincipal.Identity != null &&
                            System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated;
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

        private void LoadPackageDataIntoModel(string packageId, ProvisioningActionModel model)
        {
            var context = GetDataContext();
            DomainModel.Package package = null;

            // Get the package
            if (ProvisioningAppManager.IsTestingEnvironment)
            {
                // Process all packages in the test environment
                package = context.Packages.FirstOrDefault(p => p.Id == new Guid(packageId));
            }
            else
            {
                // Process not-preview packages in the production environment
                package = context.Packages.FirstOrDefault(p => p.Id == new Guid(packageId) && p.Preview == false);
            }

            if (package != null)
            {
                if ((package.PackageType == PackageType.Tenant &&
                    !this.Request.Url.AbsolutePath.Contains("/tenant/")) ||
                    (package.PackageType == PackageType.SiteCollection &&
                    !this.Request.Url.AbsolutePath.Contains("/site/")))
                {
                    throw new ApplicationException("Invalid request, the requested package/template is not valid for the current request!");
                }

                model.DisplayName = package.DisplayName;
                model.ActionType = package.PackageType == PackageType.SiteCollection ? ActionType.Site : ActionType.Tenant;
                model.ForceNewSite = package.ForceNewSite;
                model.MatchingSiteBaseTemplateId = package.MatchingSiteBaseTemplateId;
                model.PackageImagePreviewUrl = GetTemplatePreviewImage(package);

                // Configure content for instructions
                model.Instructions = package.Instructions;

                // If we don't have specific instructions
                if (model.Instructions == null)
                {
                    // Get the default instructions
                    var instructionsPage = context.ContentPages.FirstOrDefault(cp => cp.Id == "system/pages/GenericInstructions.md");

                    if (instructionsPage != null)
                    {
                        model.Instructions = instructionsPage.Content;
                    }
                }

                // Configure content for provisioning recap
                model.ProvisionRecap = package.ProvisionRecap;

                // If we don't have specific provisioning recap
                if (String.IsNullOrEmpty(model.ProvisionRecap))
                {
                    // Get the default provisioning recap
                    var provisionRecapPage = context.ContentPages.FirstOrDefault(cp => cp.Id == "system/pages/GenericProvisioning.md");

                    if (provisionRecapPage != null)
                    {
                        model.ProvisionRecap = provisionRecapPage.Content;
                    }
                }

                // Retrieve parameters from the package/template definition
                var packageFileUrl = new Uri(package.PackageUrl);
                var packageLocalFolder = packageFileUrl.AbsolutePath.Substring(1,
                    packageFileUrl.AbsolutePath.LastIndexOf('/') - 1);
                var packageFileName = packageFileUrl.AbsolutePath.Substring(packageLocalFolder.Length + 2);

                // If we have the package properties
                if (package.PackageProperties != null && package.PackageProperties.Length > 0)
                {
                    // Use them
                    model.PackageProperties = JsonConvert.DeserializeObject<Dictionary<String, String>>(package.PackageProperties);
                }
                else
                {
                    // Otherwise, use an empty list of parameters
                    model.PackageProperties = new Dictionary<string, string>();
                }

                // Configure the metadata properties
                var metadata = new
                {
                    properties = new[] {
                                    new {
                                        name = "",
                                        caption = "",
                                        description = "",
                                        editor = "",
                                        editorSettings = "",
                                    }
                                },
                    preRequirements = new[] {
                                    new {
                                        assemblyName = "",
                                        typeName = "",
                                        configuration = "",
                                        preRequirementContent = "",
                                    }
                                }
                };

                var metadataProperties = JsonConvert.DeserializeAnonymousType(package.PropertiesMetadata, metadata);
                model.MetadataProperties = metadataProperties.properties.ToDictionary(
                    i => i.name,
                    i => new MetadataProperty
                    {
                        Name = i.name,
                        Caption = i.caption,
                        Description = i.description,
                        Editor = i.editor,
                        EditorSettings = i.editorSettings
                    });

                model.MetadataPropertiesJson = JsonConvert.SerializeObject(model.MetadataProperties);

                if (metadataProperties.preRequirements != null && metadataProperties.preRequirements.Length > 0)
                {
                    model.ProvisioningPreRequirements = metadataProperties
                        .preRequirements
                        .Select(i => new ProvisioningPreRequirement 
                        { 
                            AssemblyName = i.assemblyName, 
                            TypeName = i.typeName,
                            Configuration = i.configuration,
                            PreRequirementContent = context.ContentPages.FirstOrDefault(cp => cp.Id == i.preRequirementContent)?.Content
                        }).ToList();
                }

                // Get the service description content
                var contentPage = context.ContentPages.FirstOrDefault(cp => cp.Id == "system/pages/ProvisioningIntro.md");

                if (contentPage != null)
                {
                    model.ProvisionDescription = contentPage.Content;
                }

                // Get the pre-reqs Header content
                var preReqsHeaderPage = context.ContentPages.FirstOrDefault(cp => cp.Id == "system/pages/CanProvisionPreReqsHeader.md");
                if (preReqsHeaderPage != null)
                {
                    model.MissingPreReqsHeader = preReqsHeaderPage.Content;
                }

                // Get the pre-reqs Footer content
                var preReqsFooterPage = context.ContentPages.FirstOrDefault(cp => cp.Id == "system/pages/CanProvisionPreReqsFooter.md");
                if (preReqsFooterPage != null)
                {
                    model.MissingPreReqsFooter = preReqsFooterPage.Content;
                }
            }
            else
            {
                throw new ApplicationException("Invalid request, the requested package/template is not available!");
            }
        }

        private async Task CheckPreRequirements(ProvisioningActionModel model, string tokenId)
        {
            var issues = new List<string>();

            var canProvisionModel = new CanProvisionModel
            {
                PackageId = model.PackageId,
                TenantId = model.TenantId,
                UserPrincipalName = model.UserPrincipalName,
                UserIsSPOAdmin = model.UserIsSPOAdmin,
                UserIsTenantAdmin = model.UserIsTenantAdmin,
                SPORootSiteUrl = model.SPORootSiteUrl,
            };

            foreach (var pr in model.ProvisioningPreRequirements)
            {
                // Try to get the type of the PreRequirements class
                var preReqType = Type.GetType($"{pr.TypeName}, {pr.AssemblyName}", true);
                var preReq = Activator.CreateInstance(preReqType) as IProvisioningPreRequirement;

                // If we've got the object instance
                if (preReq != null)
                {
                    // Validate the Pre-Requirement
                    var requirementFullfilled = await preReq.Validate(canProvisionModel, tokenId, pr.Configuration);
                    if (!requirementFullfilled)
                    {
                        // Collect any Pre-Requirement result
                        issues.Add(pr.PreRequirementContent);
                    }
                }
            }

            // Return the whole set of results, if any
            model.PreRequirementIssues = issues;
        }

        private static async Task LoadThemesFromTenant(ProvisioningActionModel model, string tokenId, SharePointSite rootSite, string graphAccessToken)
        {

            // Retrieve the SPO URL for the Admin Site
            var adminSiteUrl = rootSite.WebUrl.Replace(".sharepoint.com", "-admin.sharepoint.com");

            // Retrieve the SPO Access Token
            var spoAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                tokenId, adminSiteUrl,
                ConfigurationManager.AppSettings["ida:ClientId"],
                ConfigurationManager.AppSettings["ida:ClientSecret"],
                ConfigurationManager.AppSettings["ida:AppUrl"]);

            // Connect to SPO and retrieve the list of available Themes
            AuthenticationManager authManager = new AuthenticationManager();
            using (ClientContext spoContext = authManager.GetAzureADAccessTokenAuthenticatedContext(adminSiteUrl, spoAccessToken))
            {
                TenantAdmin.Tenant tenant = new TenantAdmin.Tenant(spoContext);
                var themes = tenant.GetAllTenantThemes();
                spoContext.Load(themes);
                spoContext.ExecuteQueryRetry();

                model.Themes = themes.Select(t => t.Name).ToList();
            }
        }

        private static String GetTemplatePreviewImage(SharePointPnP.ProvisioningApp.DomainModel.Package package)
        {
            var settings = Newtonsoft.Json.JsonConvert.DeserializeObject<SharePointPnP.ProvisioningApp.DomainModel.TemplateSettingsMetadata>(package.PropertiesMetadata);
            if (settings?.displayInfo?.previewImages != null &&
                settings?.displayInfo?.previewImages.Length > 0)
            {
                var cardPreview = settings.displayInfo.previewImages.FirstOrDefault(p => p.type == "cardpreview");
                if (cardPreview != null)
                {
                    return (cardPreview.url);
                }
            }

            return (package.ImagePreviewUrl);
        }


        private async Task<CanProvisionResult> CanProvisionInternal(CanProvisionModel model, Boolean validateUser = true)
        {
            var canProvisionResult = new CanProvisionResult();

            if (!validateUser || IsValidUser())
            {
                String provisioningScope = ConfigurationManager.AppSettings["SPPA:ProvisioningScope"];
                String provisioningEnvironment = ConfigurationManager.AppSettings["SPPA:ProvisioningEnvironment"];

                var tokenId = $"{model.TenantId}-{model.UserPrincipalName.ToLower().GetHashCode()}-{provisioningScope}-{provisioningEnvironment}";
                var graphAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                    tokenId, "https://graph.microsoft.com/");

                // Retrieve the provisioning package from the database and from the Blob Storage
                var context = GetDataContext();
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
                    if ((package.PackageType == PackageType.Tenant &&
                        !this.Request.Url.AbsolutePath.Contains("/tenant/")) ||
                        (package.PackageType == PackageType.SiteCollection &&
                        !this.Request.Url.AbsolutePath.Contains("/site/")))
                    {
                        throw new ApplicationException("Invalid request, the requested package/template is not valid for the current request!");
                    }

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
            }

            return canProvisionResult;
        }
    }
}