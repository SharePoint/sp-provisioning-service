using Microsoft.Azure;
using Microsoft.Owin.Security.Cookies;
using Microsoft.SharePoint.Client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using OfficeDevPnP.Core;
using OfficeDevPnP.Core.Framework.Provisioning.Connectors;
using OfficeDevPnP.Core.Framework.Provisioning.Model;
using OfficeDevPnP.Core.Framework.Provisioning.Providers;
using OfficeDevPnP.Core.Framework.Provisioning.Providers.Xml;
using SharePointPnP.ProvisioningApp.DomainModel;
using SharePointPnP.ProvisioningApp.Infrastructure;
using SharePointPnP.ProvisioningApp.Infrastructure.DomainModel.Provisioning;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
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
            HttpContext.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Redirect("/");
        }

        public ActionResult Index()
        {
            return View();
        }

        //[AllowAnonymous]
        //public ActionResult Error(string message)
        //{
        //    throw new Exception(message);
        //}

        [AllowAnonymous]
        public ActionResult Error(Exception exception)
        {
            HandleErrorInfo model = null;
            if (exception != null)
            {
                model = new HandleErrorInfo(exception, "unknown", "unknown");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Provision(String packageId = null)
        {
            if (String.IsNullOrEmpty(packageId))
            {
                throw new ArgumentNullException("packageId");
            }

            ProvisioningActionModel model = new ProvisioningActionModel();

            if (System.Threading.Thread.CurrentPrincipal != null &&
                System.Threading.Thread.CurrentPrincipal.Identity != null &&
                System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                var issuer = (System.Threading.Thread.CurrentPrincipal as System.Security.Claims.ClaimsPrincipal)?.FindFirst("iss");
                if (issuer != null && !String.IsNullOrEmpty(issuer.Value))
                {
                    var issuerValue = issuer.Value.Substring(0, issuer.Value.Length - 1);
                    var tenantId = issuerValue.Substring(issuerValue.LastIndexOf("/") + 1);
                    var upn = (System.Threading.Thread.CurrentPrincipal as System.Security.Claims.ClaimsPrincipal)?.FindFirst(ClaimTypes.Upn)?.Value;

                    if (this.IsAllowedUpnTenant(upn))
                    {
                        // Prepare the model data
                        model.TenantId = tenantId;
                        model.UserPrincipalName = upn;
                        model.PackageId = packageId;
                        model.ApplyTheme = false;
                        model.ApplyCustomTheme = false;

                        String provisioningScope = ConfigurationManager.AppSettings["SPPA:ProvisioningScope"];
                        String provisioningEnvironment = ConfigurationManager.AppSettings["SPPA:ProvisioningEnvironment"];

                        var tokenId = $"{model.TenantId}-{model.UserPrincipalName.GetHashCode()}-{provisioningScope}-{provisioningEnvironment}";
                        var graphAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                            tokenId, "https://graph.microsoft.com/");

                        model.UserIsTenantAdmin = Utilities.UserIsTenantGlobalAdmin(graphAccessToken);
                        model.UserIsSPOAdmin = Utilities.UserIsSPOAdmin(graphAccessToken);
                        model.NotificationEmail = upn;

                        // If the current user is an admin, we can get the available Themes
                        if (model.UserIsTenantAdmin || model.UserIsSPOAdmin)
                        {
                            // Determine the URL of the root SPO site for the current tenant
                            var rootSiteJson = HttpHelper.MakeGetRequestForString("https://graph.microsoft.com/v1.0/sites/root", graphAccessToken);
                            SharePointSite rootSite = JsonConvert.DeserializeObject<SharePointSite>(rootSiteJson);

                            // Store the SPO Root Site URL in the Model
                            model.SPORootSiteUrl = rootSite.WebUrl;

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

                        var context = GetDataContext();
                        DomainModel.Package package = null;

                        // Get the package
                        if (Boolean.Parse(ConfigurationManager.AppSettings["TestEnvironment"]))
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


                            var provider = new XMLAzureStorageTemplateProvider(
                                ConfigurationManager.AppSettings["BlobTemplatesProvider:ConnectionString"],
                                packageLocalFolder);

                            using (Stream stream = provider.Connector.GetFileStream(packageFileName))
                            {
                                // Crate a copy of the source stream
                                MemoryStream mem = new MemoryStream();
                                stream.CopyTo(mem);
                                mem.Position = 0;

                                ProvisioningHierarchy hierarchy = null;

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

                                    // Get the .xml provisioning template file name
                                    var xmlTemplateFileName = packageFileName.ToLower().Replace(".pnp", ".xml");

                                    // Get a provider based on the in-memory .PNP Open XML file
                                    XMLTemplateProvider openXmlProvider = new XMLOpenXMLTemplateProvider(
                                        new OpenXMLConnector(mem));

                                    // Get the full hierarchy
                                    hierarchy = openXmlProvider.GetHierarchy(xmlTemplateFileName);
                                }

                                // If we have the hierarchy and its parameters
                                if (hierarchy != null && hierarchy.Parameters != null)
                                {
                                    // Use them
                                    model.PackageProperties = hierarchy.Parameters;
                                }
                                else
                                {
                                    // Otherwise, use an empty list of parameters
                                    model.PackageProperties = new Dictionary<string, string>();
                                }
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
                                });

                            model.MetadataPropertiesJson = JsonConvert.SerializeObject(model.MetadataProperties);

                            // Get the service description content
                            var contentPage = context.ContentPages.FirstOrDefault(cp => cp.Id == "system/pages/ProvisioningIntro.md");

                            if (contentPage != null)
                            {
                                model.ProvisionDescription = contentPage.Content;
                            }
                        }
                        else
                        {
                            throw new ApplicationException("Invalid request, the requested package/template is not available!");
                        }
                    }
                    else
                    {
                        throw new ApplicationException("Invalid request, the current tenant is not allowed to use this solution!");
                    }
                }
            }

            return View("Provision", model);
        }

        [HttpPost]
        public async Task<ActionResult> Provision(ProvisioningActionModel model)
        {
            if (model != null && ModelState.IsValid)
            {
                if (!String.IsNullOrEmpty(model.MetadataPropertiesJson))
                {
                    model.MetadataProperties = JsonConvert.DeserializeObject<Dictionary<String, MetadataProperty>>(model.MetadataPropertiesJson);
                }

                // If there is an input file for the logo
                if (Request.Files != null && Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                {
                    // Generate a random file name
                    model.CustomLogo = $"{Guid.NewGuid()}-{Request.Files[0].FileName}";

                    // Get a reference to the blob storage account
                    var blobLogosConnectionString = ConfigurationManager.AppSettings["BlobLogosProvider:ConnectionString"];
                    var blobLogosContainerName = ConfigurationManager.AppSettings["BlobLogosProvider:ContainerName"];

                    CloudStorageAccount csaLogos;
                    if (!CloudStorageAccount.TryParse(blobLogosConnectionString, out csaLogos))
                        throw new ArgumentException("Cannot create cloud storage account from given connection string.");

                    CloudBlobClient blobLogosClient = csaLogos.CreateCloudBlobClient();
                    CloudBlobContainer containerLogos = blobLogosClient.GetContainerReference(blobLogosContainerName);

                    // Store the file in the blob storage
                    CloudBlockBlob blobLogo = containerLogos.GetBlockBlobReference(model.CustomLogo);
                    await blobLogo.UploadFromStreamAsync(Request.Files[0].InputStream);
                }

                // Get a reference to the blob storage queue
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                    CloudConfigurationManager.GetSetting("SPPA:StorageConnectionString"));

                // Get queue... create if does not exist.
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                CloudQueue queue = queueClient.GetQueueReference(
                    CloudConfigurationManager.GetSetting("SPPA:StorageQueueName"));
                queue.CreateIfNotExists();

                // add message to the queue
                queue.AddMessage(new CloudQueueMessage(JsonConvert.SerializeObject(model)));
            }

            // Get the service description content
            var context = GetDataContext();
            var contentPage = context.ContentPages.FirstOrDefault(cp => cp.Id == "system/pages/ProvisioningScheduled.md");

            if (contentPage != null)
            {
                model.ProvisionDescription = contentPage.Content;
            }

            return View("ProvisionQueued", model);
        }

        [HttpGet]
        public async Task<JsonResult> UrlIsAvailableInSPO(String url)
        {
            bool siteUrlInUse = false;

            if (System.Threading.Thread.CurrentPrincipal != null &&
                System.Threading.Thread.CurrentPrincipal.Identity != null &&
                System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                var issuer = (System.Threading.Thread.CurrentPrincipal as System.Security.Claims.ClaimsPrincipal)?.FindFirst("iss");
                if (issuer != null && !String.IsNullOrEmpty(issuer.Value))
                {
                    var issuerValue = issuer.Value.Substring(0, issuer.Value.Length - 1);
                    var tenantId = issuerValue.Substring(issuerValue.LastIndexOf("/") + 1);
                    var upn = (System.Threading.Thread.CurrentPrincipal as System.Security.Claims.ClaimsPrincipal)?.FindFirst(ClaimTypes.Upn)?.Value;

                    String provisioningScope = ConfigurationManager.AppSettings["SPPA:ProvisioningScope"];
                    String provisioningEnvironment = ConfigurationManager.AppSettings["SPPA:ProvisioningEnvironment"];

                    var tokenId = $"{tenantId}-{upn.GetHashCode()}-{provisioningScope}-{provisioningEnvironment}";
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

                    // Connect to SPO and retrieve the list of available Themes
                    AuthenticationManager authManager = new AuthenticationManager();
                    using (ClientContext spoContext = authManager.GetAzureADAccessTokenAuthenticatedContext(rootSiteUrl, spoAccessToken))
                    {
                        siteUrlInUse = spoContext.WebExistsFullUrl($"{rootSiteUrl.TrimEnd(new char[] { '/' })}{url}");
                    }
                }
            }

            return (Json(new { result = !siteUrlInUse }, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet));
        }

        private bool IsAllowedUpnTenant(string upn)
        {
            if (Boolean.Parse(ConfigurationManager.AppSettings["TestEnvironment"]))
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
    }
}