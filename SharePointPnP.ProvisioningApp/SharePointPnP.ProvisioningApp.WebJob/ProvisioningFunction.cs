using Microsoft.Azure.WebJobs;
using Microsoft.Online.SharePoint.TenantAdministration;
using Microsoft.SharePoint.Client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using OfficeDevPnP.Core;
using OfficeDevPnP.Core.Framework.Provisioning.Connectors;
using OfficeDevPnP.Core.Framework.Provisioning.Model;
using OfficeDevPnP.Core.Framework.Provisioning.ObjectHandlers;
using OfficeDevPnP.Core.Framework.Provisioning.Providers;
using OfficeDevPnP.Core.Framework.Provisioning.Providers.Xml;
using OfficeDevPnP.Core.Utilities.Themes;
using SharePointPnP.ProvisioningApp.DomainModel;
using SharePointPnP.ProvisioningApp.Infrastructure;
using SharePointPnP.ProvisioningApp.Infrastructure.DomainModel.Provisioning;
using SharePointPnP.ProvisioningApp.Infrastructure.Mail;
using SharePointPnP.ProvisioningApp.Infrastructure.Telemetry;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SharePointPnP.ProvisioningApp.WebJob
{
    public static class ProvisioningFunction
    {
        public static async Task RunAsync([QueueTrigger("actions")]ProvisioningActionModel action, TextWriter log)
        {
            log.WriteLine($"Processing queue trigger function for {action.UserPrincipalName} on tenant {action.TenantId}");

            // Instantiate and use the telemetry model
            TelemetryUtility telemetry = new TelemetryUtility(log);
            Dictionary<string, string> telemetryProperties = new Dictionary<string, string>();

            // Configure telemetry properties
            // telemetryProperties.Add("UserPrincipalName", action.UserPrincipalName);
            // telemetryProperties.Add("TenantId", action.TenantId);
            telemetryProperties.Add("PnPCorrelationId", action.CorrelationId.ToString());

            var appOnlyAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAppOnlyAccessTokenAsync(
                "https://graph.microsoft.com/",
                ConfigurationManager.AppSettings["OfficeDevPnP:TenantId"],
                ConfigurationManager.AppSettings["OfficeDevPnP:ClientId"],
                ConfigurationManager.AppSettings["OfficeDevPnP:ClientSecret"],
                ConfigurationManager.AppSettings["OfficeDevPnP:AppUrl"]);

            // Get a reference to the data context
            ProvisioningAppDBContext dbContext = new ProvisioningAppDBContext();

            // Retrieve any additional recipient for exceptions alerts
            var additionalRecipients = dbContext.NotificationRecipients.Where(r => r.Enabled).Select(r => r.Email).ToArray();

            try
            {
                // Log telemetry event
                telemetry?.LogEvent("ProvisioningFunction.Start");

                String provisioningEnvironment = ConfigurationManager.AppSettings["SPPA:ProvisioningEnvironment"];
                var tokenId = $"{action.TenantId}-{action.UserPrincipalName.GetHashCode()}-{action.ActionType.ToString().ToLower()}-{provisioningEnvironment}";

                // Retrieve the SPO target tenant via Microsoft Graph
                var graphAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                    tokenId, "https://graph.microsoft.com/",
                    ConfigurationManager.AppSettings[$"{action.ActionType}:ClientId"],
                    ConfigurationManager.AppSettings[$"{action.ActionType}:ClientSecret"],
                    ConfigurationManager.AppSettings[$"{action.ActionType}:AppUrl"]);

                if (!String.IsNullOrEmpty(graphAccessToken))
                {
                    #region Get current context data (User, SPO Tenant, SPO Access Token)

                    // Get the currently connected user name and email (UPN)
                    var jwtAccessToken = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(graphAccessToken);

                    String delegatedUPN = String.Empty;
                    var upnClaim = jwtAccessToken.Claims.FirstOrDefault(c => c.Type == "upn");
                    if (upnClaim != null && !String.IsNullOrEmpty(upnClaim.Value))
                    {
                        delegatedUPN = upnClaim.Value;
                    }

                    String delegatedUserName = String.Empty;
                    var nameClaim = jwtAccessToken.Claims.FirstOrDefault(c => c.Type == "name");
                    if (nameClaim != null && !String.IsNullOrEmpty(nameClaim.Value))
                    {
                        delegatedUserName = nameClaim.Value;
                    }

                    // Determine the URL of the root SPO site for the current tenant
                    var rootSiteJson = HttpHelper.MakeGetRequestForString("https://graph.microsoft.com/v1.0/sites/root", graphAccessToken);
                    SharePointSite rootSite = JsonConvert.DeserializeObject<SharePointSite>(rootSiteJson);

                    String spoTenant = rootSite.WebUrl;

                    log.WriteLine($"Target SharePoint Online Tenant: {spoTenant}");

                    // Configure telemetry properties
                    telemetryProperties.Add("SPOTenant", spoTenant);

                    // Retrieve the SPO Access Token
                    var spoAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                        tokenId, rootSite.WebUrl,
                        ConfigurationManager.AppSettings[$"{action.ActionType}:ClientId"],
                        ConfigurationManager.AppSettings[$"{action.ActionType}:ClientSecret"],
                        ConfigurationManager.AppSettings[$"{action.ActionType}:AppUrl"]);
                    log.WriteLine($"Retrieved target SharePoint Online Access Token.");

                    #endregion

                    // Connect to SPO, create and provision site
                    AuthenticationManager authManager = new AuthenticationManager();
                    using (ClientContext context = authManager.GetAzureADAccessTokenAuthenticatedContext(spoTenant, spoAccessToken))
                    {
                        var web = context.Web;
                        context.Load(web, w => w.Title);
                        await context.ExecuteQueryRetryAsync();

                        log.WriteLine($"SharePoint Online Root Site Collection title: {web.Title}");

                        #region Store the main site URL in KeyVault

                        // Store the main site URL in KeyVault
                        var vault = new KeyVaultService();

                        // Read any existing properties for the current tenantId
                        var properties = await vault.GetAsync(tokenId);

                        if (properties == null)
                        {
                            // If there are no properties, create a new dictionary
                            properties = new Dictionary<String, String>();
                        }

                        // Set/Update the RefreshToken value
                        properties["SPORootSite"] = spoTenant;

                        // Add or Update the Key Vault accordingly
                        await vault.AddOrUpdateAsync(tokenId, properties);

                        #endregion

                        #region Provision the package

                        var package = dbContext.Packages.FirstOrDefault(p => p.Id == new Guid(action.PackageId));

                        if (package != null)
                        {
                            // Update the Popularity of the package
                            package.TimesApplied++;
                            dbContext.SaveChanges();

                            #region Get the Provisioning Hierarchy file

                            // Determine reference path variables
                            var blobConnectionString = ConfigurationManager.AppSettings["BlobTemplatesProvider:ConnectionString"];
                            var blobContainerName = ConfigurationManager.AppSettings["BlobTemplatesProvider:ContainerName"];

                            var packageFileName = package.PackageUrl.Substring(package.PackageUrl.LastIndexOf('/') + 1);
                            var packageFileUri = new Uri(package.PackageUrl);
                            var packageFileRelativePath = packageFileUri.AbsolutePath.Substring(2 + blobContainerName.Length);
                            var packageFileRelativeFolder = packageFileRelativePath.Substring(0, packageFileRelativePath.LastIndexOf('/'));

                            // Configure telemetry properties
                            telemetryProperties.Add("PackageFileName", packageFileName);
                            telemetryProperties.Add("PackageFileUri", packageFileUri.ToString());

                            // Read the main provisioning file from the Blob Storage
                            CloudStorageAccount csa;
                            if (!CloudStorageAccount.TryParse(blobConnectionString, out csa))
                                throw new ArgumentException("Cannot create cloud storage account from given connection string.");

                            CloudBlobClient blobClient = csa.CreateCloudBlobClient();
                            CloudBlobContainer blobContainer = blobClient.GetContainerReference(blobContainerName);

                            var blockBlob = blobContainer.GetBlockBlobReference(packageFileRelativePath);

                            // Crate an in-memory copy of the source stream
                            MemoryStream mem = new MemoryStream();
                            await blockBlob.DownloadToStreamAsync(mem);
                            mem.Position = 0;

                            // Prepare the output hierarchy
                            ProvisioningHierarchy hierarchy = null;

                            if (packageFileName.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase))
                            {
                                // That's an XML Provisioning Template file

                                XDocument xml = XDocument.Load(mem);
                                mem.Position = 0;

                                // Deserialize the stream into a provisioning hierarchy reading any 
                                // dependecy with the Azure Blob Storage connector
                                var formatter = XMLPnPSchemaFormatter.GetSpecificFormatter(xml.Root.Name.NamespaceName);
                                var templateLocalFolder = $"{blobContainerName}/{packageFileRelativeFolder}";

                                var provider = new XMLAzureStorageTemplateProvider(
                                    blobConnectionString,
                                    templateLocalFolder);
                                formatter.Initialize(provider);

                                // Get the full hierarchy
                                hierarchy = ((IProvisioningHierarchyFormatter)formatter).ToProvisioningHierarchy(mem);
                                hierarchy.Connector = provider.Connector;
                            }
                            else if (packageFileName.EndsWith(".pnp", StringComparison.InvariantCultureIgnoreCase))
                            {
                                // That's a PnP Package file

                                // Get a provider based on the in-memory .PNP Open XML file
                                OpenXMLConnector openXmlConnector = new OpenXMLConnector(mem);
                                XMLTemplateProvider provider = new XMLOpenXMLTemplateProvider(
                                    openXmlConnector);

                                // Get the .xml provisioning template file name
                                var xmlTemplateFileName = openXmlConnector.Info?.Properties?.TemplateFileName ??
                                    packageFileName.Substring(packageFileName.LastIndexOf('/') + 1)
                                    .ToLower().Replace(".pnp", ".xml");

                                // Get the full hierarchy
                                hierarchy = provider.GetHierarchy(xmlTemplateFileName);
                                hierarchy.Connector = provider.Connector;
                            }

                            #endregion

                            #region Apply the template

                            // Prepare variable to collect provisioned sites
                            var provisionedSites = new List<Tuple<String, String>>();

                            // If we have a hierarchy with at least one Sequence
                            if (hierarchy != null && hierarchy.Sequences != null && hierarchy.Sequences.Count > 0)
                            {
                                Console.WriteLine($"Provisioning hierarchy \"{hierarchy.DisplayName}\"");

                                var tenantUrl = UrlUtilities.GetTenantAdministrationUrl(context.Url);

                                // Retrieve the SPO Access Token
                                var spoAdminAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                                    tokenId, tenantUrl,
                                    ConfigurationManager.AppSettings[$"{action.ActionType}:ClientId"],
                                    ConfigurationManager.AppSettings[$"{action.ActionType}:ClientSecret"],
                                    ConfigurationManager.AppSettings[$"{action.ActionType}:AppUrl"]);
                                log.WriteLine($"Retrieved target SharePoint Online Admin Center Access Token.");

                                using (var tenantContext = authManager.GetAzureADAccessTokenAuthenticatedContext(tenantUrl, spoAdminAccessToken))
                                {
                                    using (var pnpTenantContext = PnPClientContext.ConvertFrom(tenantContext))
                                    {
                                        var tenant = new Microsoft.Online.SharePoint.TenantAdministration.Tenant(pnpTenantContext);

                                        // Prepare logging for hierarchy application
                                        var ptai = new ProvisioningTemplateApplyingInformation();
                                        ptai.MessagesDelegate += delegate (string message, ProvisioningMessageType messageType) {
                                            log.WriteLine($"{messageType} - {message}");
                                        };
                                        ptai.ProgressDelegate += delegate (string message, int step, int total) {
                                            log.WriteLine($"{step:00}/{total:00} - {message}");
                                        };
                                        ptai.SiteProvisionedDelegate += delegate (string title, string url)
                                        {
                                            log.WriteLine($"Fully provisioned site '{title}' with URL: {url}");
                                            var provisionedSite = new Tuple<string, string>(title, url);
                                            if (!provisionedSites.Contains(provisionedSite))
                                            {
                                                provisionedSites.Add(provisionedSite);
                                            }
                                        };

                                        // Configure the OAuth Access Tokens for the client context
                                        ptai.AccessTokens.Add(new Uri(tenantUrl).Authority, spoAdminAccessToken);
                                        ptai.AccessTokens.Add(new Uri(spoTenant).Authority, spoAccessToken);

                                        // Configure the OAuth Access Tokens for the PnPClientContext, too
                                        pnpTenantContext.PropertyBag["AccessTokens"] = ptai.AccessTokens;

                                        #region Theme handling

                                        // Process the graphical Theme
                                        if (action.ApplyTheme)
                                        {
                                            // If we don't have any custom Theme
                                            if (!action.ApplyCustomTheme)
                                            {
                                                // Associate the selected already existing Theme to all the sites of the hierarchy
                                                foreach (var sc in hierarchy.Sequences[0].SiteCollections)
                                                {
                                                    sc.Theme = action.SelectedTheme;
                                                    foreach (var s in sc.Sites)
                                                    {
                                                        UpdateChildrenSitesTheme(s, action.SelectedTheme);
                                                    }
                                                }
                                            }
                                        }

                                        #endregion

                                        // Configure provisioning parameters
                                        if (action.PackageProperties != null)
                                        {
                                            foreach (var key in action.PackageProperties.Keys)
                                            {
                                                if (hierarchy.Parameters.ContainsKey(key.ToString()))
                                                {
                                                    hierarchy.Parameters[key.ToString()] = action.PackageProperties[key].ToString();
                                                }
                                                else
                                                {
                                                    hierarchy.Parameters.Add(key.ToString(), action.PackageProperties[key].ToString());
                                                }

                                                // Configure telemetry properties
                                                telemetryProperties.Add($"PackageProperty.{key}", action.PackageProperties[key].ToString());
                                            }
                                        }

                                        // Log telemetry event
                                        telemetry?.LogEvent("ProvisioningFunction.BeginProvisioning", telemetryProperties);

                                        // Apply the hierarchy
                                        log.WriteLine($"Hierarchy Provisioning Started: {DateTime.Now:hh.mm.ss}");
                                        tenant.ApplyProvisionHierarchy(hierarchy, hierarchy.Sequences[0].ID, ptai);
                                        log.WriteLine($"Hierarchy Provisioning Completed: {DateTime.Now:hh.mm.ss}");

                                        if(action.ApplyTheme && action.ApplyCustomTheme)
                                        {
                                            if (!String.IsNullOrEmpty(action.ThemePrimaryColor) &&
                                                !String.IsNullOrEmpty(action.ThemeBodyTextColor) &&
                                                !String.IsNullOrEmpty(action.ThemeBodyBackgroundColor))
                                            {
                                                log.WriteLine($"Applying custom Theme to provisioned sites");

                                                #region Palette generation for Theme

                                                var jsonPalette = ThemeUtility.GetThemeAsJSON(
                                                    action.ThemePrimaryColor,
                                                    action.ThemeBodyTextColor,
                                                    action.ThemeBodyBackgroundColor);

                                                #endregion

                                                // Apply the custom theme to all of the provisioned sites
                                                foreach (var ps in provisionedSites)
                                                {
                                                    using (var provisionedSiteContext = authManager.GetAzureADAccessTokenAuthenticatedContext(ps.Item2, spoAccessToken))
                                                    {
                                                        if (provisionedSiteContext.Web.ApplyTheme(jsonPalette))
                                                        {
                                                            log.WriteLine($"Custom Theme applied on site '{ps.Item1}' with URL: {ps.Item2}");
                                                        }
                                                        else
                                                        {
                                                            log.WriteLine($"Failed to apply custom Theme on site '{ps.Item1}' with URL: {ps.Item2}");
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        // Log telemetry event
                                        telemetry?.LogEvent("ProvisioningFunction.EndProvisioning", telemetryProperties);

                                        // Notify user about the provisioning outcome
                                        MailHandler.SendMailNotification(
                                            "ProvisioningCompleted",
                                            action.NotificationEmail,
                                            null,
                                            new
                                            {
                                                TemplateName = action.DisplayName,
                                                ProvisionedSites = provisionedSites,
                                            },
                                            appOnlyAccessToken);
                                    }
                                }
                            }
                            else
                            {
                                throw new ApplicationException($"The requested package does not contain a valid PnP Hierarchy!");
                            }

                            #endregion
                        }
                        else
                        {
                            throw new ApplicationException($"Cannot find the package with ID: {action.PackageId}");
                        }

                        #endregion

                        log.WriteLine($"Function successfully executed!");
                        // Log telemetry event
                        telemetry?.LogEvent("ProvisioningFunction.End", telemetryProperties);
                    }
                }
                else
                {
                    var noTokensErrorMessage = $"Cannot retrieve Refresh Token or Access Token for {action.UserPrincipalName} in tenant {action.TenantId}!";
                    log.WriteLine(noTokensErrorMessage);
                    throw new ApplicationException(noTokensErrorMessage);
                }
            }
            catch (Exception ex)
            {
                telemetry?.LogException(ex, "ProvisioningFunction.RunAsync", telemetryProperties);

                // Notify user about the provisioning outcome
                MailHandler.SendMailNotification(
                    "ProvisioningFailed",
                    action.NotificationEmail,
                    additionalRecipients,
                    new {
                        TemplateName = action.DisplayName,
                        ExceptionDetails = ex.ToDetailedString(),
                        PnPCorrelationId = action.CorrelationId.ToString(),
                    },
                    appOnlyAccessToken);

                throw ex;
            }
            finally
            {
                telemetry?.Flush();
            }
        }

        // Method to recursively update the Theme of all children sites
        private static void UpdateChildrenSitesTheme(SubSite site, string themeName)
        {
            site.Theme = themeName;
            foreach (var s in site.Sites)
            {
                UpdateChildrenSitesTheme(s, themeName);
            }
        }
    }
}
