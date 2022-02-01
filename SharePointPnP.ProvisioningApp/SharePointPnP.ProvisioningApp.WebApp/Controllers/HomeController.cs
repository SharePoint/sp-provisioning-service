﻿//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using PnP.Framework;
using PnP.Framework.Provisioning.CanProvisionRules;
using PnP.Framework.Provisioning.Connectors;
using PnP.Framework.Provisioning.Model;
using PnP.Framework.Provisioning.ObjectHandlers;
using PnP.Framework.Provisioning.Providers;
using PnP.Framework.Provisioning.Providers.Xml;
using SharePointPnP.ProvisioningApp.DomainModel;
using SharePointPnP.ProvisioningApp.Infrastructure;
using SharePointPnP.ProvisioningApp.Infrastructure.DomainModel.Provisioning;
using SharePointPnP.ProvisioningApp.Infrastructure.Security;
using SharePointPnP.ProvisioningApp.WebApp.Models;
using SharePointPnP.ProvisioningApp.WebApp.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
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
        public async Task<ActionResult> Provision(String packageId = null, String returnUrl = null, String source = null)
        {
            if (String.IsNullOrEmpty(packageId))
            {
                throw new ArgumentNullException("packageId");
            }

            if (String.IsNullOrEmpty(source))
            {
                source = "default";
            }

            CheckBetaFlag();
            PrepareHeaderData(returnUrl);
            LogSourceTracking(source, 0, Request.Url.ToString(), packageId); // 0 = PageView

            ProvisioningActionModel model = new ProvisioningActionModel();

            try
            {
                if (IsValidUser())
                {
                    // Resolve current user's UPN and TenantId
                    var (upn, tenantId) = AuthHelper.GetCurrentUserIdentityClaims(
                        System.Security.Claims.ClaimsPrincipal.Current);

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

                        var graphAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                            MsalAppBuilder.BuildConfidentialClientApplication(),
                            AuthenticationConfig.GetGraphScopes());

                        model.UserIsTenantAdmin = Utilities.UserIsTenantGlobalAdmin(graphAccessToken);
                        model.UserIsSPOAdmin = Utilities.UserIsSPOAdmin(graphAccessToken);
                        model.NotificationEmail = upn;

                        model.ReturnUrl = returnUrl;
                        model.Source = source;

                        #endregion

                        // Determine the URL of the root SPO site for the current tenant
                        var rootSiteJson = HttpHelper.MakeGetRequestForString("https://graph.microsoft.com/v1.0/sites/root", graphAccessToken);
                        SharePointSite rootSite = JsonConvert.DeserializeObject<SharePointSite>(rootSiteJson);

                        // Store the SPO Root Site URL in the Model
                        model.SPORootSiteUrl = rootSite.WebUrl;

                        // Try to get the Access Tokens
                        var accessTokens = await PrepareAccessTokensAsync(model);

                        // If we are missing any of the access tokens, we force a re-consent
                        if (accessTokens != null &&
                            accessTokens.Any(i => string.IsNullOrEmpty(i.Value)))
                        {
                            var consentUrl = string.Format(AuthenticationConfig.AdminConsentFormat,
                                tenantId,
                                AuthenticationConfig.ClientId,
                                string.Empty,
                                AuthenticationConfig.RedirectUri
                                );
                            return new RedirectResult(consentUrl);
                        }

                        // If the current user is an admin, we can get the available Themes
                        if (model.UserIsTenantAdmin || model.UserIsSPOAdmin)
                        {
                            await LoadThemesFromTenant(model, rootSite, graphAccessToken);
                        }

                        LoadPackageDataIntoModel(packageId, model);

                        if (model.ProvisioningPreRequirements != null)
                        {
                            await CheckPreRequirements(model);
                        }
                    }
                    else
                    {
                        throw new ApplicationException("Invalid request, the current tenant is not allowed to use this solution!");
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
            LogSourceTracking(model.Source, 1, Request.Url.ToString(), model.PackageId); // 1 = Provisioning

            if (model != null && ModelState.IsValid)
            {
                // Enrich the model with Access Tokens
                model.AccessTokens = await PrepareAccessTokensAsync(model);

                // If we are missing any of the access tokens, we force a re-consent
                if (model.AccessTokens != null &&
                    model.AccessTokens.Any(i => string.IsNullOrEmpty(i.Value)))
                {
                    var consentUrl = string.Format(AuthenticationConfig.AdminConsentFormat,
                        model.TenantId,
                        AuthenticationConfig.ClientId,
                        string.Empty,
                        AuthenticationConfig.RedirectUri
                        );
                    return new RedirectResult(consentUrl);
                }

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

        private static async Task<Dictionary<string, string>> PrepareAccessTokensAsync(ProvisioningActionModel model)       {
            // Prepare the variable to hold the result
            var accessTokens = new Dictionary<string, string>();

            // Retrieve the Microsoft Graph Access Token
            var graphAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                MsalAppBuilder.BuildConfidentialClientApplication(),
                AuthenticationConfig.GetGraphScopes());

            // Retrieve the SPO Access Token
            var spoAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                MsalAppBuilder.BuildConfidentialClientApplication(),
                AuthenticationConfig.GetSpoScopes(model.SPORootSiteUrl));

            // Retrieve the SPO URL for the Admin Site
            var adminSiteUrl = model.SPORootSiteUrl.Replace(".sharepoint.com", "-admin.sharepoint.com");

            // Retrieve the SPO Access Token
            var spoAdminAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                MsalAppBuilder.BuildConfidentialClientApplication(),
                AuthenticationConfig.GetSpoScopes(adminSiteUrl));

            // Configure the resulting dictionary
            accessTokens.Add(new Uri(AuthenticationConfig.GraphBaseUrl).Authority, graphAccessToken);
            accessTokens.Add(new Uri(model.SPORootSiteUrl).Authority, spoAccessToken);
            accessTokens.Add(new Uri(adminSiteUrl).Authority, spoAdminAccessToken);

            return accessTokens;
        }

        [HttpGet]
        public async Task<JsonResult> UrlIsAvailableInSPO(String url)
        {
            bool siteUrlInUse = false;
            string baseTemplateId = null;

            if (IsValidUser())
            {
                var graphAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                    MsalAppBuilder.BuildConfidentialClientApplication(),
                    AuthenticationConfig.GetGraphScopes());

                // Determine the URL of the root SPO site for the current tenant
                var rootSiteJson = HttpHelper.MakeGetRequestForString("https://graph.microsoft.com/v1.0/sites/root", graphAccessToken);
                SharePointSite rootSite = JsonConvert.DeserializeObject<SharePointSite>(rootSiteJson);

                var rootSiteUrl = rootSite.WebUrl;

                // Retrieve the SPO Access Token
                var spoAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                    MsalAppBuilder.BuildConfidentialClientApplication(),
                    AuthenticationConfig.GetSpoScopes(rootSiteUrl));

                // Connect to SPO and check if the URL is available or not
                AuthenticationManager authManager = new AuthenticationManager();
                using (ClientContext spoContext = authManager.GetAccessTokenContext(rootSiteUrl, spoAccessToken))
                {
                    var targetUrl = $"{rootSiteUrl.TrimEnd(new char[] { '/' })}{url}";
                    siteUrlInUse = spoContext.WebExistsFullUrl(targetUrl);
                    if (siteUrlInUse)
                    {
                        using (ClientContext spoTargetSiteContext = authManager.GetAccessTokenContext(targetUrl, spoAccessToken))
                        {
                            baseTemplateId = spoTargetSiteContext.Web.GetBaseTemplateId();
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
        [AllowAnonymous]
        public ActionResult CategoriesMenu(String returnUrl = null, String source = null)
        {
            CategoriesMenuViewModel model = new CategoriesMenuViewModel();

            // Let's see if we need to filter the output categories
            var slbHost = ConfigurationManager.AppSettings["SPLBSiteHost"];
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

            var queryCategories = context.Categories
                .AsNoTracking()
                .Where(c => c.Packages.Any(
                    p => p.Visible &&
                    (testEnvironment || !p.Preview) &&
                    p.TargetPlatforms.Any(pf => pf.Id == targetPlatform)
                ))
                .OrderBy(c => c.Order)
                .Include("Packages")
                .Include("Packages.TargetPlatforms")
                .ToList();

            // Cleanup packages
            var tempCategories = queryCategories;
            for (int cIndex = 0; cIndex < tempCategories.Count; cIndex++)
            {
                var c = tempCategories[cIndex];
                for (int pIndex = 0; pIndex < c.Packages.Count; pIndex++)
                {
                    var p = c.Packages[pIndex];
                    if (!p.Visible ||
                        (p.Preview && !testEnvironment) ||
                        (!p.TargetPlatforms.Any(pf => pf.Id == targetPlatform)))
                    {
                        queryCategories[cIndex].Packages.RemoveAt(pIndex);
                        pIndex--;
                    }
                }
            }

            model.Categories = queryCategories;
            model.Source = source;

            return PartialView("CategoriesMenu", model);
        }

        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<ActionResult> ProvisionContentPack(ProvisionContentPackRequest provisionRequest)
        //{
        //    var provisionResponse = new ProvisionContentPackResponse();

        //    // If the input paramenters are missing, raise a BadRequest response
        //    if (provisionRequest == null)
        //    {
        //        return ThrowEmptyRequest();
        //    }

        //    // If the TenantId input argument is missing, raise a BadRequest response
        //    if (String.IsNullOrEmpty(provisionRequest.TenantId))
        //    {
        //        return ThrowMissingArgument("TenantId");
        //    }

        //    // If the UserPrincipalName input argument is missing, raise a BadRequest response
        //    if (String.IsNullOrEmpty(provisionRequest.UserPrincipalName))
        //    {
        //        return ThrowMissingArgument("UserPrincipalName");
        //    }

        //    // If the PackageId input argument is missing, raise a BadRequest response
        //    if (String.IsNullOrEmpty(provisionRequest.PackageId))
        //    {
        //        return ThrowMissingArgument("PackageId");
        //    }

        //    if (provisionRequest != null &&
        //        !String.IsNullOrEmpty(provisionRequest.TenantId) &&
        //        !String.IsNullOrEmpty(provisionRequest.UserPrincipalName) &&
        //        !String.IsNullOrEmpty(provisionRequest.PackageId))
        //    {
        //        try
        //        {
        //            // Validate the Package ID
        //            var context = GetDataContext();
        //            DomainModel.Package package = null;

        //            // Get the package
        //            if (ProvisioningAppManager.IsTestingEnvironment)
        //            {
        //                // Process all packages in the test environment
        //                package = context.Packages.FirstOrDefault(p => p.Id == new Guid(provisionRequest.PackageId));
        //            }
        //            else
        //            {
        //                // Process not-preview packages in the production environment
        //                package = context.Packages.FirstOrDefault(p => p.Id == new Guid(provisionRequest.PackageId) && p.Preview == false);
        //            }

        //            // If the package is not valid
        //            if (package == null)
        //            {
        //                // Throw an exception accordingly
        //                throw new ArgumentException("Invalid Package Id!");
        //            }

        //            String provisioningScope = package.PackageType == PackageType.Tenant ? "tenant" : "site";
        //            String provisioningEnvironment = ConfigurationManager.AppSettings["SPPA:ProvisioningEnvironment"];

        //            try
        //            {
        //                // Retrieve the refresh token and store it in the KeyVault
        //                await ProvisioningAppManager.AccessTokenProvider.SetupSecurityFromAuthorizationCodeAsync(
        //                    provisionRequest.AuthorizationCode,
        //                    provisionRequest.TenantId,
        //                    AuthenticationConfig.ClientId,
        //                    AuthenticationConfig.ClientSecret,
        //                    AuthenticationConfig.GraphBaseUrl,
        //                    provisionRequest.RedirectUri);
        //            }
        //            catch (Exception ex)
        //            {
        //                // In case of any authorization exception, raise an Unauthorized exception
        //                return ThrowUnauthorized(ex);
        //            }

        //            // First of all, validate the provisioning request for pre-requirements
        //            provisionResponse.CanProvisionResult = await CanProvisionInternal(
        //                new CanProvisionModel
        //                {
        //                    PackageId = provisionRequest.PackageId,
        //                    TenantId = provisionRequest.TenantId,
        //                    UserPrincipalName = provisionRequest.UserPrincipalName,
        //                    SPORootSiteUrl = provisionRequest.SPORootSiteUrl,
        //                    UserIsSPOAdmin = true, // We assume that the request comes from an Admin
        //                    UserIsTenantAdmin = true, // We assume that the request comes from an Admin
        //                });

        //            // If the package can be provisioned onto the target
        //            if (provisionResponse.CanProvisionResult.CanProvision)
        //            {
        //                // Prepare the provisioning request
        //                var request = new ProvisioningActionModel();
        //                request.ActionType = ActionType.Tenant; // Do we want to support site/tenant or just one?
        //                request.ApplyCustomTheme = false;
        //                request.ApplyTheme = false; // Do we need to apply any special theme?
        //                request.CorrelationId = Guid.NewGuid();
        //                request.CustomLogo = null;
        //                request.DisplayName = "Provision Content Pack";
        //                request.PackageId = provisionRequest.PackageId;
        //                request.TargetSiteAlreadyExists = false; // Do we want to check this?
        //                request.TargetSiteBaseTemplateId = null;
        //                request.TenantId = provisionRequest.TenantId;
        //                request.UserIsSPOAdmin = true; // We don't use this in the job
        //                request.UserIsTenantAdmin = true; // We don't use this in the job
        //                request.UserPrincipalName = provisionRequest.UserPrincipalName.ToLower();
        //                request.NotificationEmail = provisionRequest.UserPrincipalName.ToLower();
        //                request.PackageProperties = provisionRequest.Parameters;

        //                // Configure the Access Tokens for the request
        //                request.AccessTokens = await PrepareAccessTokensAsync(request);

        //                // Enqueue the provisioning request
        //                await ProvisioningAppManager.EnqueueProvisioningRequest(request);

        //                // Set the status of the provisioning request
        //                provisionResponse.ProvisioningStarted = true;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            // In case of any other exception, raise an InternalServerError exception
        //            return ThrowInternalServerError(ex);
        //        }
        //    }

        //    // Return to the requested URL
        //    return Json(provisionResponse);
        //}

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

        private void LogSourceTracking(string source, int action, string url, string packageId)
        {
            // Prepare the Source Tracking event data
            var sourceTrackingEvent = new
            {
                SourceId = source,
                SourceTrackingAction = action,
                SourceTrackingUrl = url,
                SourceTrackingFromProduction = !ProvisioningAppManager.IsTestingEnvironment,
                TemplateId = packageId
            };

            try
            {
                // Make the Azure Function call for reporting
                HttpHelper.MakePostRequest(ConfigurationManager.AppSettings["SPPA:SourceTrackingFunctionUrl"],
                    sourceTrackingEvent, "application/json", null);
            }
            catch
            {
                // Intentionally ignore any reporting issue
            }
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
            return System.Security.Claims.ClaimsPrincipal.Current != null &&
                            System.Security.Claims.ClaimsPrincipal.Current.Identity != null &&
                            System.Security.Claims.ClaimsPrincipal.Current.Identity.IsAuthenticated;
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
                model.ForceExistingSite = package.ForceExistingSite;
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

                // Retrieve provisioning text messages
                GetProvisionTextMessages(package, model);

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
                                },
                    postActions = new[] {
                                    new {
                                        assemblyName = "",
                                        typeName = "",
                                        configuration = "",
                                    }
                                },
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

                // Configure the pre-requirements
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

                // Configure the post-actions
                if (metadataProperties.postActions != null && metadataProperties.postActions.Length > 0)
                {
                    model.ProvisioningPostActionsJson = JsonConvert.SerializeObject(metadataProperties
                        .postActions
                        .Select(i => new ProvisioningPostAction
                        {
                            AssemblyName = i.assemblyName,
                            TypeName = i.typeName,
                            Configuration = i.configuration,
                        }).ToList());
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

        private async Task CheckPreRequirements(ProvisioningActionModel model)
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
                    var requirementFullfilled = await preReq.Validate(canProvisionModel, pr.Configuration);
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

        private static async Task LoadThemesFromTenant(ProvisioningActionModel model, SharePointSite rootSite, string graphAccessToken)
        {

            // Retrieve the SPO URL for the Admin Site
            var adminSiteUrl = rootSite.WebUrl.Replace(".sharepoint.com", "-admin.sharepoint.com");

            // Retrieve the SPO Access Token
            var spoAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                MsalAppBuilder.BuildConfidentialClientApplication(),
                AuthenticationConfig.GetSpoScopes(adminSiteUrl));

            // Connect to SPO and retrieve the list of available Themes
            AuthenticationManager authManager = new AuthenticationManager();
            using (ClientContext spoContext = authManager.GetAccessTokenContext(adminSiteUrl, spoAccessToken))
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

        private static void GetProvisionTextMessages(SharePointPnP.ProvisioningApp.DomainModel.Package package, ProvisioningActionModel model)
        {
            var settings = Newtonsoft.Json.JsonConvert.DeserializeObject<SharePointPnP.ProvisioningApp.DomainModel.TemplateSettingsMetadata>(package.PropertiesMetadata);
            if (settings?.displayInfo?.provisionMessages != null)
            {
                model.ProvisionPageTitle = settings.displayInfo.provisionMessages.provisionPageTitle;
                model.ProvisionPageSubTitle = settings.displayInfo.provisionMessages.provisionPageSubTitle;
                model.ProvisionPageText = settings.displayInfo.provisionMessages.provisionPageText;
            }
        }

        private async Task<CanProvisionResult> CanProvisionInternal(CanProvisionModel model, Boolean validateUser = true)
        {
            var canProvisionResult = new CanProvisionResult();

            if (!validateUser || IsValidUser())
            {
                String provisioningScope = ConfigurationManager.AppSettings["SPPA:ProvisioningScope"];
                String provisioningEnvironment = ConfigurationManager.AppSettings["SPPA:ProvisioningEnvironment"];

                var graphAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                    MsalAppBuilder.BuildConfidentialClientApplication(),
                    AuthenticationConfig.GetGraphScopes());

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

                        // Store the Graph Access Token for any further context cloning
                        if (!string.IsNullOrEmpty(graphAccessToken))
                        {
                            accessTokens.Add(new Uri("https://graph.microsoft.com/").Authority, graphAccessToken);
                        }

                        AuthenticationManager authManager = new AuthenticationManager();
                        var ptai = new ProvisioningTemplateApplyingInformation();

                        // Retrieve the SPO URL for the Admin Site
                        var rootSiteUrl = model.SPORootSiteUrl;

                        // Retrieve the SPO Access Token for SPO
                        var spoAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                            MsalAppBuilder.BuildConfidentialClientApplication(),
                            AuthenticationConfig.GetSpoScopes(rootSiteUrl));

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
                                var resourceUri = $"https://{r}";

                                // Try to get a fresh new Access Token
                                var token = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                                    MsalAppBuilder.BuildConfidentialClientApplication(),
                                    resourceUri.Equals(AuthenticationConfig.GraphBaseUrl, StringComparison.InvariantCultureIgnoreCase) ?
                                        AuthenticationConfig.GetGraphScopes() :
                                        AuthenticationConfig.GetSpoScopes(resourceUri));

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
                                    MsalAppBuilder.BuildConfidentialClientApplication(),
                                    AuthenticationConfig.GetSpoScopes(adminSiteUrl));

                                // Store the SPO Admin Access Token for any further context cloning
                                accessTokens.Add(new Uri(adminSiteUrl).Authority, spoAdminAccessToken);

                                // Connect to SPO Admin Site and evaluate the CanProvision rules for the hierarchy
                                using (var tenantContext = authManager.GetAccessTokenContext(adminSiteUrl, spoAdminAccessToken))
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
                                using (var clientContext = authManager.GetAccessTokenContext(rootSiteUrl, spoAccessToken))
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