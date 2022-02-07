//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System;
using System.Configuration;
using System.Globalization;
using System.Linq;

namespace SharePointPnP.ProvisioningApp.Infrastructure.Security
{
    public static class AuthenticationConfig
    {
        public const string IssuerClaim = "iss";
        public const string GraphBaseUrl = "https://graph.microsoft.com/";
        public const string TenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";
        public const string AdminConsentFormat = "https://login.microsoftonline.com/{0}/adminconsent?client_id={1}&state={2}&redirect_uri={3}";
        public const string BasicSignInScopes = "openid profile email offline_access";
        public const string NameClaimType = "name";

        private static string ProvisioningScope { get; } = ConfigurationManager.AppSettings["SPPA:ProvisioningScope"] ?? Environment.GetEnvironmentVariable("SPPA:ProvisioningScope");

        /// <summary>
        /// The Client ID is used by the application to uniquely identify itself to Azure AD.
        /// </summary>
        public static string ClientId { get; } = ConfigurationManager.AppSettings[$"{ProvisioningScope}:ClientId"] ?? Environment.GetEnvironmentVariable($"{ProvisioningScope}:ClientId");

        /// <summary>
        /// The ClientSecret is a credential used to authenticate the application to Azure AD.  Azure AD supports password and certificate credentials.
        /// </summary>
        public static string ClientSecret { get; } = ConfigurationManager.AppSettings[$"{ProvisioningScope}:ClientSecret"] ?? Environment.GetEnvironmentVariable($"{ProvisioningScope}:ClientSecret");

        /// <summary>
        /// The Redirect Uri is the URL where the user will be redirected after they sign in.
        /// </summary>
        public static string RedirectUri { get; } = ConfigurationManager.AppSettings[$"{ProvisioningScope}:AppUrl"] ?? Environment.GetEnvironmentVariable($"{ProvisioningScope}:AppUrl");

        /// <summary>
        /// The AAD Instance for Azure Active Directory authentication
        /// </summary>
        public static string AADInstance { get; } = ConfigurationManager.AppSettings["ida:AADInstance"] ?? Environment.GetEnvironmentVariable("ida:AADInstance");

        /// <summary>
        /// The API Audience for API Authentication
        /// </summary>
        public static string Audience { get; } = ConfigurationManager.AppSettings["ida:Audience"] ?? Environment.GetEnvironmentVariable("ida:Audience");

        /// <summary>
        /// The API Permission Scope per API Authentication and Authorization
        /// </summary>
        public static string ApiScope { get; } = ConfigurationManager.AppSettings["ida:Scope"] ?? Environment.GetEnvironmentVariable("ida:Scope");

        /// <summary>
        /// The authority
        /// </summary>
        public static string Authority = $"{AADInstance}common/v2.0";

        /// <summary>
        /// String with list of Microsoft Graph permission scopes
        /// </summary>
        public static string GraphScopes { get; } = ConfigurationManager.AppSettings[$"{ProvisioningScope}:GraphScopes"] ?? Environment.GetEnvironmentVariable($"{ProvisioningScope}:GraphScopes");

        /// <summary>
        /// String with list of SharePoint Online permission scopes
        /// </summary>
        public static string SPOScopes { get; } = ConfigurationManager.AppSettings[$"{ProvisioningScope}:SpoScopes"] ?? Environment.GetEnvironmentVariable($"{ProvisioningScope}:SpoScopes");

        public static string[] GetGraphScopes()
        {
            return (from s in GraphScopes.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                    select $"{GraphBaseUrl}{s}").ToArray();
        }

        public static string[] GetSpoScopes(string spoBaseUrl)
        {
            var spoUrl = spoBaseUrl.EndsWith("/") ? spoBaseUrl : $"{spoBaseUrl}/";

            return (from s in SPOScopes.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                    select $"{spoUrl}{s}").ToArray();
        }
    }
}