//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.SharePoint.Client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using PnP.Framework;
using PnP.Framework.Provisioning.Connectors;
using PnP.Framework.Provisioning.Model;
using PnP.Framework.Provisioning.ObjectHandlers;
using PnP.Framework.Provisioning.Providers;
using PnP.Framework.Provisioning.Providers.Xml;
using PnP.Framework.Utilities.Themes;
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
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SharePointPnP.ProvisioningApp.Functions
{
    public static class ProvisioningFunction
    {
        private static KnownExceptions knownExceptions;

        static ProvisioningFunction()
        {
            // Get the JSON settings for known exceptions
            Stream stream = typeof(KnownExceptions)
                .Assembly
                .GetManifestResourceStream("SharePointPnP.ProvisioningApp.WebJobServiceBus.known-exceptions.json");

            // If we have the stream and it can be read
            if (stream != null && stream.CanRead)
            {
                using (var sr = new StreamReader(stream))
                {
                    // Deserialize it
                    knownExceptions = JsonConvert.DeserializeObject<KnownExceptions>(sr.ReadToEnd());
                }
            }
        }

        [Function("ProvisioningFunction")]
        public static async Task Run([ServiceBusTrigger("actions", Connection = "SPPA:ServiceBusConnectionString")] string queueItem, FunctionContext functionContext)
        {
            var logger = functionContext.GetLogger("ProvisioningFunction");
            var action = JsonConvert.DeserializeObject<ProvisioningActionModel>(queueItem);

            var startProvisioning = DateTime.Now;

            String provisioningEnvironment = Environment.GetEnvironmentVariable("SPPA:ProvisioningEnvironment");

            logger.LogInformationWithPnPCorrelation("Processing queue trigger function for tenant {TenantId}", action.CorrelationId, action.TenantId);

            // Instantiate and use the telemetry model
            TelemetryUtility telemetry = new TelemetryUtility((s) => {
                logger.LogInformationWithPnPCorrelation(s, action.CorrelationId);
            });
            Dictionary<string, string> telemetryProperties = new Dictionary<string, string>();

            // Configure telemetry properties
            // telemetryProperties.Add("UserPrincipalName", action.UserPrincipalName);
            telemetryProperties.Add("TenantId", action.TenantId);
            telemetryProperties.Add("PnPCorrelationId", action.CorrelationId.ToString());
            telemetryProperties.Add("TargetSiteAlreadyExists", action.TargetSiteAlreadyExists.ToString());
            telemetryProperties.Add("TargetSiteBaseTemplateId", action.TargetSiteBaseTemplateId);

            // Get a reference to the data context
            ProvisioningAppDBContext dbContext = new ProvisioningAppDBContext();

            try
            {
                // Log telemetry event
                telemetry?.LogEvent("ProvisioningFunction.Start");

                if (CheckIfActionIsAlreadyRunning(action, dbContext))
                {
                    throw new ConcurrentProvisioningException("The requested package is currently provisioning in the selected target tenant and cannot be applied in parallel. Please wait for the previous provisioning action to complete.");
                }

                // Initialize the container of access tokens, if needed
                if (action.AccessTokens == null)
                {
                    action.AccessTokens = new Dictionary<string, string>();
                }

                // Retrieve the SPO target tenant via Microsoft Graph
                logger.LogInformationWithPnPCorrelation("Retrieving target Microsoft Graph Access Token.", action.CorrelationId);
                var graphAccessToken =
                    action.AccessTokens.ContainsKey("graph.microsoft.com") ?
                    action.AccessTokens["graph.microsoft.com"] : null;


                if (!String.IsNullOrEmpty(graphAccessToken))
                {
                    logger.LogInformationWithPnPCorrelation("Retrieved target Microsoft Graph Access Token.", action.CorrelationId);

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

                    logger.LogInformationWithPnPCorrelation("Target SharePoint Online Tenant: {SPOTenant}", action.CorrelationId, spoTenant);

                    // Configure telemetry properties
                    telemetryProperties.Add("SPOTenant", spoTenant);

                    // Retrieve the SPO Access Token
                    var spoAuthority = new Uri(rootSite.WebUrl).Authority;
                    logger.LogInformationWithPnPCorrelation($"Retrieving target SharePoint Online Access Token for {spoAuthority}.", action.CorrelationId);
                    var spoAccessToken =
                        action.AccessTokens.ContainsKey(spoAuthority) ?
                        action.AccessTokens[spoAuthority] : null;

                    #endregion

                    if (string.IsNullOrEmpty(spoAccessToken))
                    {
                        throw new ApplicationException($"Failed to retrieve target SharePoint Online Access Token for {spoAuthority}.");
                    }

                    logger.LogInformationWithPnPCorrelation($"Retrieved target SharePoint Online Access Token for {spoAuthority}.", action.CorrelationId);

                    // Connect to SPO, create and provision site
                    AuthenticationManager authManager = new AuthenticationManager();
                    using (ClientContext context = authManager.GetAccessTokenContext(spoTenant, spoAccessToken))
                    {
                        // Telemetry and startup
                        var web = context.Web;
                        context.ClientTag = $"SPDev:ProvisioningPortal-{provisioningEnvironment}";
                        context.Load(web, w => w.Title, w => w.Id);
                        await context.ExecuteQueryAsync();

                        // Save the current SPO Correlation ID
                        telemetryProperties.Add("SPOCorrelationId", context.TraceCorrelationId);

                        logger.LogInformationWithPnPCorrelation("SharePoint Online Root Site Collection title: {WebTitle}", action.CorrelationId, web.Title);

                        #region Provision the package

                        var package = dbContext.Packages.FirstOrDefault(p => p.Id == new Guid(action.PackageId));

                        if (package != null)
                        {
                            // Update the Popularity of the package
                            package.TimesApplied++;
                            dbContext.SaveChanges();

                            #region Get the Provisioning Hierarchy file

                            logger.LogInformationWithPnPCorrelation("Reading the Provisioning Hierarchy file {PackageUrl}", action.CorrelationId, package.PackageUrl);

                            // Determine reference path variables
                            var blobConnectionString = Environment.GetEnvironmentVariable("BlobTemplatesProvider:ConnectionString");
                            var blobContainerName = Environment.GetEnvironmentVariable("BlobTemplatesProvider:ContainerName");

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

                            logger.LogInformationWithPnPCorrelation("Read the Provisioning Hierarchy file {PackageUrl}", action.CorrelationId, package.PackageUrl);

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

                            logger.LogInformationWithPnPCorrelation("Provisioning Hierarchy ready for processing", action.CorrelationId);

                            #endregion

                            #region Apply the template

                            // Prepare variable to collect provisioned sites
                            var provisionedSites = new List<Tuple<String, String>>();

                            // If we have a hierarchy with at least one Sequence
                            if (hierarchy != null) // && hierarchy.Sequences != null && hierarchy.Sequences.Count > 0)
                            {
                                Console.WriteLine($"Provisioning hierarchy \"{hierarchy.DisplayName}\"");

                                var tenantUrl = UrlUtilities.GetTenantAdministrationUrl(context.Url);

                                // Retrieve the SPO Access Token
                                var spoAdminAuthority = new Uri(tenantUrl).Authority;
                                logger.LogInformationWithPnPCorrelation($"Retrieving target SharePoint Online Admin Center Access Token for {spoAdminAuthority}.", action.CorrelationId);
                                var spoAdminAccessToken =
                                    action.AccessTokens.ContainsKey(spoAdminAuthority) ?
                                    action.AccessTokens[spoAdminAuthority] : null;

                                if (string.IsNullOrEmpty(spoAdminAccessToken))
                                {
                                    throw new ApplicationException($"Failed to retrieve target SharePoint Online Admin Center Access Token for {spoAdminAuthority}.");
                                }

                                logger.LogInformationWithPnPCorrelation($"Retrieved target SharePoint Online Admin Center Access Token for {spoAdminAuthority}.", action.CorrelationId);

                                using (var tenantContext = authManager.GetAccessTokenContext(tenantUrl, spoAdminAccessToken))
                                {
                                    using (var pnpTenantContext = PnPClientContext.ConvertFrom(tenantContext))
                                    {
                                        var tenant = new Microsoft.Online.SharePoint.TenantAdministration.Tenant(pnpTenantContext);

                                        // Prepare a dictionary to hold the access tokens
                                        var accessTokens = new Dictionary<String, String>();

                                        // Prepare logging for hierarchy application
                                        var ptai = new ProvisioningTemplateApplyingInformation();
                                        ptai.MessagesDelegate += delegate (string message, ProvisioningMessageType messageType)
                                        {
                                            logger.LogInformationWithPnPCorrelation($"{messageType} - {message.Replace("{", "{{").Replace("}", "}}")}", action.CorrelationId);
                                        };
                                        ptai.ProgressDelegate += delegate (string message, int step, int total)
                                        {
                                            logger.LogInformationWithPnPCorrelation($"{step:00}/{total:00} - {message.Replace("{", "{{").Replace("}", "}}")}", action.CorrelationId);
                                        };
                                        ptai.SiteProvisionedDelegate += delegate (string title, string url)
                                        {
                                            logger.LogInformationWithPnPCorrelation("Fully provisioned site '{SiteTitle}' with URL: {SiteUrl}", action.CorrelationId, title, url);
                                            var provisionedSite = new Tuple<string, string>(title, url);
                                            if (!provisionedSites.Contains(provisionedSite))
                                            {
                                                provisionedSites.Add(provisionedSite);
                                            }
                                        };

                                        // Configure the OAuth Access Tokens for the client context
                                        accessTokens.Add(new Uri(tenantUrl).Authority, spoAdminAccessToken);
                                        accessTokens.Add(new Uri(spoTenant).Authority, spoAccessToken);

                                        // Configure the OAuth Access Tokens for the PnPClientContext, too
                                        pnpTenantContext.PropertyBag["AccessTokens"] = accessTokens;
                                        ptai.AccessTokens = accessTokens;

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

                                        // Define a PnPProvisioningContext scope to share the security context across calls
                                        using (var pnpProvisioningContext = new PnPProvisioningContext(async (r, s) =>
                                        {
                                            if (accessTokens.ContainsKey(r))
                                            {
                                                // In this scenario we just use the dictionary of access tokens
                                                // in fact the overall operation for sure will take less than 1 hour
                                                return await Task.FromResult(accessTokens[r]);
                                            }
                                            else
                                            {
                                                return null;
                                            }
                                        }))
                                        {
                                            // Configure the webhooks, if any
                                            if (action.Webhooks != null && action.Webhooks.Count > 0)
                                            {
                                                foreach (var t in hierarchy.Templates)
                                                {
                                                    foreach (var wh in action.Webhooks)
                                                    {
                                                        AddProvisioningTemplateWebhook(t, wh, ProvisioningTemplateWebhookKind.ProvisioningTemplateStarted);
                                                        AddProvisioningTemplateWebhook(t, wh, ProvisioningTemplateWebhookKind.ObjectHandlerProvisioningStarted);
                                                        AddProvisioningTemplateWebhook(t, wh, ProvisioningTemplateWebhookKind.ObjectHandlerProvisioningCompleted);
                                                        AddProvisioningTemplateWebhook(t, wh, ProvisioningTemplateWebhookKind.ProvisioningTemplateCompleted);
                                                        AddProvisioningTemplateWebhook(t, wh, ProvisioningTemplateWebhookKind.ExceptionOccurred);
                                                    }
                                                }

                                                foreach (var wh in action.Webhooks)
                                                {
                                                    AddProvisioningWebhook(hierarchy, wh, ProvisioningTemplateWebhookKind.ProvisioningStarted);
                                                    AddProvisioningWebhook(hierarchy, wh, ProvisioningTemplateWebhookKind.ProvisioningCompleted);
                                                    AddProvisioningWebhook(hierarchy, wh, ProvisioningTemplateWebhookKind.ProvisioningExceptionOccurred);
                                                }
                                            }

                                            // Disable the WebSettings handler for non-admin users
                                            //if (!TenantExtensions.IsCurrentUserTenantAdmin(context))

                                            // Disable the WebSettings handler for provisionings with site level permissions
                                            if (action.ActionType == ActionType.Site)
                                            {
                                                ptai.HandlersToProcess &= ~Handlers.WebSettings;
                                            }

                                            // Apply the hierarchy
                                            logger.LogInformationWithPnPCorrelation("Hierarchy Provisioning Started: {ProvisioningStartDateTime}", action.CorrelationId, DateTime.Now.ToString("hh.mm.ss"));
                                            tenant.ApplyProvisionHierarchy(hierarchy,
                                                (hierarchy.Sequences != null && hierarchy.Sequences.Count > 0) ?
                                                hierarchy.Sequences[0].ID : null,
                                                ptai);
                                            logger.LogInformationWithPnPCorrelation("Hierarchy Provisioning Completed: {ProvisioningEndDateTime}", action.CorrelationId, DateTime.Now.ToString("hh.mm.ss"));
                                        }

                                        if (action.ApplyTheme && action.ApplyCustomTheme)
                                        {
                                            if (!String.IsNullOrEmpty(action.ThemePrimaryColor) &&
                                                !String.IsNullOrEmpty(action.ThemeBodyTextColor) &&
                                                !String.IsNullOrEmpty(action.ThemeBodyBackgroundColor))
                                            {
                                                logger.LogInformationWithPnPCorrelation("Applying custom Theme to provisioned sites", action.CorrelationId);

                                                #region Palette generation for Theme

                                                var jsonPalette = ThemeUtility.GetThemeAsJSON(
                                                    action.ThemePrimaryColor,
                                                    action.ThemeBodyTextColor,
                                                    action.ThemeBodyBackgroundColor);

                                                #endregion

                                                // Apply the custom theme to all of the provisioned sites
                                                foreach (var ps in provisionedSites)
                                                {
                                                    using (var provisionedSiteContext = authManager.GetAccessTokenContext(ps.Item2, spoAccessToken))
                                                    {
                                                        if (provisionedSiteContext.Web.ApplyTheme(jsonPalette))
                                                        {
                                                            logger.LogInformationWithPnPCorrelation($"Custom Theme applied on site '{ps.Item1}' with URL: {ps.Item2}", action.CorrelationId);
                                                        }
                                                        else
                                                        {
                                                            logger.LogInformationWithPnPCorrelation($"Failed to apply custom Theme on site '{ps.Item1}' with URL: {ps.Item2}", action.CorrelationId);
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        #region Process any Post-Action

                                        // If we have the URL of the provisioned site
                                        // and if we have any Post-Action
                                        if (provisionedSites != null && provisionedSites.Count > 0 &&
                                            !string.IsNullOrEmpty(action.ProvisioningPostActionsJson))
                                        {
                                            var provisioningPostActions = JsonConvert.DeserializeObject<List<ProvisioningPostAction>>(action.ProvisioningPostActionsJson);

                                            foreach (var postAction in provisioningPostActions)
                                            {
                                                var postActionTelemetryProperties = new Dictionary<string, string>(telemetryProperties);
                                                postActionTelemetryProperties.Add("PostAction", postAction.TypeName);

                                                // Log telemetry event
                                                telemetry?.LogEvent("ProvisioningFunction.PostAction", postActionTelemetryProperties);

                                                await ProcessPostAction(accessTokens, provisionedSites[0].Item2, postAction);
                                            }
                                        }

                                        #endregion

                                        // Log telemetry event
                                        telemetry?.LogEvent("ProvisioningFunction.EndProvisioning", telemetryProperties);

                                        // Notify user about the provisioning outcome
                                        if (!String.IsNullOrEmpty(action.NotificationEmail))
                                        {
                                            var appOnlyAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAppOnlyAccessTokenAsync(
                                                "https://graph.microsoft.com/",
                                                Environment.GetEnvironmentVariable("OfficeDevPnP:TenantId"),
                                                Environment.GetEnvironmentVariable("OfficeDevPnP:ClientId"),
                                                Environment.GetEnvironmentVariable("OfficeDevPnP:ClientSecret"),
                                                Environment.GetEnvironmentVariable("OfficeDevPnP:AppUrl"));

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

                                        // Log reporting event (1 = Success)
                                        LogReporting(action, provisioningEnvironment, startProvisioning, package, 1);

                                        // Log source tracking for provisioned sites
                                        LogSourceTrackingProvisionedSites(action, spoAccessToken, authManager, provisionedSites);
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

                        #region Process any children items

                        // If there are children items
                        if (action.ChildrenItems != null && action.ChildrenItems.Count > 0)
                        {
                            // Prepare any further child provisioning request
                            action.PackageId = action.ChildrenItems[0].PackageId;
                            action.PackageProperties = action.ChildrenItems[0].Parameters;
                            action.ChildrenItems.RemoveAt(0);

                            // Enqueue any further child provisioning request
                            await ProvisioningAppManager.EnqueueProvisioningRequest(action);
                        }

                        #endregion

                        logger.LogInformationWithPnPCorrelation("Function successfully executed!", action.CorrelationId);
                        // Log telemetry event
                        telemetry?.LogEvent("ProvisioningFunction.End", telemetryProperties);
                    }
                }
                else
                {
                    var noTokensErrorMessage = $"Cannot retrieve Refresh Token or Access Token for action {action.CorrelationId} in tenant {action.TenantId}!";
                    logger.LogInformationWithPnPCorrelation(noTokensErrorMessage, action.CorrelationId);
                    throw new ApplicationException(noTokensErrorMessage);
                }
            }
            catch (Exception ex)
            {
                // Skip logging exception for Recycled Site
                if (ex is RecycledSiteException)
                {
                    // Log reporting event (3 = RecycledSite)
                    LogReporting(action, provisioningEnvironment, startProvisioning, null, 3, ex.ToDetailedString());

                    // rather log an event
                    telemetry?.LogEvent("ProvisioningFunction.RecycledSite", telemetryProperties);
                }
                // Skip logging exception for Concurrent Provisioning 
                else if (ex is ConcurrentProvisioningException)
                {
                    // Log reporting event (4 = ConcurrentProvisioningException)
                    LogReporting(action, provisioningEnvironment, startProvisioning, null, 4, ex.ToDetailedString());

                    // rather log an event
                    telemetry?.LogEvent("ProvisioningFunction.ConcurrentProvisioning", telemetryProperties);
                }
                else
                {
                    // Log reporting event (2 = Failed)
                    LogReporting(action, provisioningEnvironment, startProvisioning, null, 2, ex.ToDetailedString());

                    // Log telemetry event
                    telemetry?.LogException(ex, "ProvisioningFunction.Failed", telemetryProperties);
                }

                if (!String.IsNullOrEmpty(action.NotificationEmail))
                {
                    var appOnlyAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAppOnlyAccessTokenAsync(
                        "https://graph.microsoft.com/",
                        Environment.GetEnvironmentVariable("OfficeDevPnP:TenantId"),
                        Environment.GetEnvironmentVariable("OfficeDevPnP:ClientId"),
                        Environment.GetEnvironmentVariable("OfficeDevPnP:ClientSecret"),
                        Environment.GetEnvironmentVariable("OfficeDevPnP:AppUrl"));

                    // Notify user about the provisioning outcome
                    MailHandler.SendMailNotification(
                        "ProvisioningFailed",
                        action.NotificationEmail,
                        null,
                        new
                        {
                            TemplateName = action.DisplayName,
                            ExceptionDetails = SimplifyException(ex),
                            PnPCorrelationId = action.CorrelationId.ToString(),
                        },
                        appOnlyAccessToken);
                }

                ProcessWebhooksExceptionNotification(action, ex);

                // Track the failure in the local action log
                MarkCurrentActionItemAsFailed(action, dbContext);

                throw ex;
            }
            finally
            {
                // Try to cleanup the pending action item, if any
                CleanupCurrentActionItem(action, dbContext);

                telemetry?.Flush();
            }
        }

        /// <summary>
        /// Notifies an exception through the configured webhooks
        /// </summary>
        /// <param name="action">The provisioning action</param>
        /// <param name="ex">The exception that occurred</param>
        private static void ProcessWebhooksExceptionNotification(ProvisioningActionModel action, Exception ex)
        {
            if (action.Webhooks != null && action.Webhooks.Count > 0)
            {
                foreach (var wh in action.Webhooks)
                {
                    var provisioningWebhook = new PnP.Framework.Provisioning.Model.ProvisioningWebhook
                    {
                        Kind = ProvisioningTemplateWebhookKind.ExceptionOccurred,
                        Url = wh.Url,
                        Method = (ProvisioningTemplateWebhookMethod)Enum.Parse(typeof(ProvisioningTemplateWebhookMethod), wh.Method.ToString(), true),
                        BodyFormat = ProvisioningTemplateWebhookBodyFormat.Json, // force JSON format
                        Async = false, // force sync webhooks
                        Parameters = wh.Parameters,
                    };

                    var httpClient = new HttpClient();

                    WebhookSender.InvokeWebhook(provisioningWebhook, httpClient,
                        ProvisioningTemplateWebhookKind.ExceptionOccurred,
                        exception: ex);
                }
            }
        }

        private static void AddProvisioningTemplateWebhook(ProvisioningTemplate template,
            Infrastructure.DomainModel.Provisioning.ProvisioningWebhook webhook, ProvisioningTemplateWebhookKind kind)
        {
            template.ProvisioningTemplateWebhooks.Add(new ProvisioningTemplateWebhook
            {
                Kind = kind,
                Url = webhook.Url,
                Method = (ProvisioningTemplateWebhookMethod)Enum.Parse(typeof(ProvisioningTemplateWebhookMethod), webhook.Method.ToString(), true),
                BodyFormat = ProvisioningTemplateWebhookBodyFormat.Json, // force JSON format
                Async = false, // force sync webhooks
                Parameters = webhook.Parameters,
            });
        }

        private static void AddProvisioningWebhook(ProvisioningHierarchy hierarchy,
            Infrastructure.DomainModel.Provisioning.ProvisioningWebhook webhook, ProvisioningTemplateWebhookKind kind)
        {
            hierarchy.ProvisioningWebhooks.Add(new PnP.Framework.Provisioning.Model.ProvisioningWebhook
            {
                Kind = kind,
                Url = webhook.Url,
                Method = (ProvisioningTemplateWebhookMethod)Enum.Parse(typeof(ProvisioningTemplateWebhookMethod), webhook.Method.ToString(), true),
                BodyFormat = ProvisioningTemplateWebhookBodyFormat.Json, // force JSON format
                Async = false, // force sync webhooks
                Parameters = webhook.Parameters,
            });
        }

        private static Boolean CheckIfActionIsAlreadyRunning(ProvisioningActionModel action, ProvisioningAppDBContext dbContext)
        {
            var result = false;

            var tenantId = Guid.Parse(action.TenantId);
            var packageId = Guid.Parse(action.PackageId);

            // Check if there is already a pending action item with the same settings and not yet expired
            var alreadyExistingItems = from i in dbContext.ProvisioningActionItems
                                       where i.TenantId == tenantId && i.PackageId == packageId
                                            && i.ExpiresOn > DateTime.Now
                                            && i.FailedOn == null
                                       select i;

            // Prepare the action properties as JSON
            var currentActionProperties = action.PackageProperties != null ? JsonConvert.SerializeObject(action.PackageProperties) : null;

            // Verify if the same package, with the same properties is already running in the same tenant
            foreach (var item in alreadyExistingItems)
            {
                if (item.PackageProperties == currentActionProperties)
                {
                    result = true;
                    break;
                }
            }

            if (!result)
            {
                // Add a ProvisioningActionItem record for tracking purposes
                dbContext.ProvisioningActionItems.Add(new ProvisioningActionItem
                {
                    Id = action.CorrelationId,
                    PackageId = packageId,
                    TenantId = tenantId,
                    PackageProperties = action.PackageProperties != null ? JsonConvert.SerializeObject(action.PackageProperties) : null,
                    CreatedOn = DateTime.Now,
                    ExpiresOn = DateTime.Now.AddHours(2),
                });
                dbContext.SaveChanges();
            }

            return (result);
        }

        private static void MarkCurrentActionItemAsFailed(ProvisioningActionModel action, ProvisioningAppDBContext dbContext)
        {
            // Check if there is the action item for the current action
            var existingItem = dbContext.ProvisioningActionItems.FirstOrDefault(i => i.Id == action.CorrelationId);

            // And in case it does exist
            if (existingItem != null)
            {
                // Set the failure date and time
                existingItem.FailedOn = DateTime.Now;

                // Update the persistence storage
                dbContext.SaveChanges();
            }
        }

        private static void CleanupCurrentActionItem(ProvisioningActionModel action, ProvisioningAppDBContext dbContext)
        {
            // Check if there is the action item for the current action
            var existingItem = dbContext.ProvisioningActionItems.FirstOrDefault(i => i.Id == action.CorrelationId);

            // And in case it does exist and it is not failed
            if (existingItem != null && !existingItem.FailedOn.HasValue)
            {
                // Delete it
                dbContext.ProvisioningActionItems.Remove(existingItem);
                dbContext.SaveChanges();
            }
        }

        private static String SimplifyException(Exception ex)
        {
            var knownException = knownExceptions?.Exceptions?.Find(e => ex.GetType().FullName == e.ExceptionType && ex.StackTrace.Contains(e.MatchingText));

            if (knownException != null)
            {
                var tmp = typeof(FriendlyErrorMessages).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

                return (typeof(FriendlyErrorMessages).GetProperty(knownException.FriendlyMessageResourceKey,
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)?.GetValue(null, null) as String);
            }
            else
            {
                return (ex.Message);
                //return (ex.ToDetailedString());
            }
        }

        private static void LogReporting(ProvisioningActionModel action, string provisioningEnvironment, DateTime startProvisioning, DomainModel.Package package, Int32 outcome, String details = null)
        {
            // Prepare the reporting event
            var provisioningEvent = new
            {
                EventId = action.CorrelationId,
                EventStartDateTime = startProvisioning,
                EventEndDateTime = DateTime.Now,
                EventOutcome = outcome,
                EventDetails = details,
                EventFromProduction = provisioningEnvironment.ToUpper() == "PROD" ? 1 : 0,
                TemplateId = action.PackageId,
                TemplateDisplayName = package?.DisplayName,
            };

            try
            {
                // Make the Azure Function call for reporting
                HttpHelper.MakePostRequest(Environment.GetEnvironmentVariable("SPPA:ReportingFunctionUrl"),
                    provisioningEvent, "application/json", null);
            }
            catch
            {
                // Intentionally ignore any reporting issue
            }
        }

        private static void LogSourceTrackingProvisionedSites(ProvisioningActionModel action, string spoAccessToken, AuthenticationManager authManager, List<Tuple<string, string>> provisionedSites)
        {
            // Log source tracking (2 = Provisioned) for every provisioned site
            foreach (var provisionedSite in provisionedSites)
            {
                // Get the provisioned site URL
                var provisionedSiteUrl = provisionedSite.Item2;

                // Connect to the provisioned site
                using (var provisionedSiteContext = authManager.GetAccessTokenContext(provisionedSiteUrl, spoAccessToken))
                {
                    // Retrieve the Site ID
                    var provisionedSiteId = provisionedSiteContext.Site.EnsureProperty(s => s.Id);

                    // Log the source tracking event
                    LogSourceTracking(action.Source, 2, null, action.PackageId, action.TenantId, provisionedSiteId.ToString());
                }
            }
        }

        private static void LogSourceTracking(string source, int action, string url, string packageId, string tenantId, string siteId)
        {
            // Prepare the Source Tracking event data
            var sourceTrackingEvent = new
            {
                SourceId = source,
                SourceTrackingAction = action,
                SourceTrackingUrl = url,
                SourceTrackingFromProduction = !ProvisioningAppManager.IsTestingEnvironment,
                TemplateId = packageId,
                TenantId = tenantId,
                SiteId = siteId,
            };

            try
            {
                // Make the Azure Function call for reporting
                HttpHelper.MakePostRequest(Environment.GetEnvironmentVariable("SPPA:SourceTrackingFunctionUrl"),
                    sourceTrackingEvent, "application/json", null);
            }
            catch
            {
                // Intentionally ignore any reporting issue
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

        private static void LogInformationWithPnPCorrelation(this ILogger logger, string message, Guid correlationId, params object[] args)
        {
            message += " [{PnPCorrelationId}]";
            if (args == null)
            {
                args = new object[0];
            }
            args = args.Append(correlationId).ToArray();
            if (logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information))
            {
                logger.LogInformation(message, args);
            }
        }

        private static async Task ProcessPostAction(Dictionary<string, string> accessTokens, string targetSiteUrl, ProvisioningPostAction postAction)
        {
            // Try to get the type of the Post-Action class
            var postActionType = Type.GetType($"{postAction.TypeName}, {postAction.AssemblyName}", true);
            var postActionItem = Activator.CreateInstance(postActionType) as IProvisioningPostAction;

            // If we've got the object instance
            if (postActionItem != null)
            {
                // Validate the Pre-Requirement
                await postActionItem.Execute(accessTokens, targetSiteUrl, postAction.Configuration);
            }
        }
    }
}
