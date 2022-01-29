//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Infrastructure.Security
{
    public class AccessTokenProvider : IAccessTokenProvider
    {
        private static string aadInstance = EnsureTrailingSlash(AuthenticationConfig.AADInstance);
        
        private const string Oid = "oid";
        private const string Tid = "tid";
        private const string ObjectId = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        private const string TenantId = "http://schemas.microsoft.com/identity/claims/tenantid";

        // We keep global client app
        private static Lazy<IConfidentialClientApplication> lazyClientApp = new Lazy<IConfidentialClientApplication>(() => { 
            
            var result = ConfidentialClientApplicationBuilder
                .Create(AuthenticationConfig.ClientId)
                .WithClientSecret(AuthenticationConfig.ClientSecret)
                .WithRedirectUri(AuthenticationConfig.RedirectUri)
                .WithAuthority(AadAuthorityAudience.AzureAdMultipleOrgs, false)
                .Build();

            // After the ConfidentialClientApplication is created, we overwrite its default UserTokenCache serialization with our implementation
            // clientapp.AddInMemoryTokenCache();

            result.AddDistributedTokenCache(services =>
            {
                services.AddDistributedMemoryCache();
                services.Configure<MsalDistributedTokenCacheAdapterOptions>(o =>
                {
                    o.Encrypt = true;
                });
            });

            return result;
        }, 
            true);

        private static string EnsureTrailingSlash(string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }

            if (!value.EndsWith("/", StringComparison.Ordinal))
            {
                return value + "/";
            }

            return value;
        }

        public async Task<String> GetAccessTokenAsync(string[] scopes)
        {
            string clientId = AuthenticationConfig.ClientId;
            string clientSecret = AuthenticationConfig.ClientSecret;
            string appUri = AuthenticationConfig.RedirectUri;

            return (await this.GetAccessTokenAsync(clientId, clientSecret, appUri, scopes));
        }

        public async Task<String> GetAccessTokenAsync(String clientId, String clientSecret, String appUri, string[] scopes)
        {
            var clientApp = lazyClientApp.Value;

            try
            {
                // Retrieve the current user's account
                var account = await clientApp.GetAccountAsync(GetAccountId(ClaimsPrincipal.Current));

                // Now retrieve the requested Access Token
                var authenticationResult = await clientApp.AcquireTokenSilent(scopes, account).ExecuteAsync().ConfigureAwait(false);

                // Return the retrieved Access Token
                return authenticationResult.AccessToken;
            }
            catch
            {
                return null;
            }
        }

        public async Task<String> GetAppOnlyAccessTokenAsync(string resourceUri, String tenantId, String clientId, String clientSecret, String appUri)
        {
            // We need to manually retrieve a valid app-only AccessToken from ClientId and ClientSecret
            using (var client = new HttpClient())
            {
                // Prepare the AAD OAuth request URI
                var tokenUri = new Uri($"{aadInstance}{tenantId}/oauth2/token");

                // Prepare the OAuth 2.0 request for an Access Token with Authorization Code
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("resource", resourceUri),
                });

                // Make the HTTP request
                var result = await client.PostAsync(tokenUri, content);
                string jsonToken = await result.Content.ReadAsStringAsync();

                // Get back the OAuth 2.0 response
                var token = JsonConvert.DeserializeObject<OAuthTokenResponse>(jsonToken);

                return (token?.AccessToken);
            }
        }

        public async Task<Dictionary<string, string>> SetupSecurityFromAuthorizationCodeAsync(String authorizationCode, String tenantId, String clientId, String clientSecret, String resourceUri, String redirectUri, String spoRootSiteUrl)
        {
            Dictionary<string, string> accessTokens = null;
            var clientApp = ConfidentialClientApplicationBuilder
                .Create(AuthenticationConfig.ClientId)
                .WithClientSecret(AuthenticationConfig.ClientSecret)
                .WithRedirectUri(redirectUri)
                .WithAuthority(AadAuthorityAudience.AzureAdMultipleOrgs, false)
                .Build();

            try
            {
                // Try to authenticate user via AuthCode
                var authenticationResult = await clientApp.AcquireTokenByAuthorizationCode(AuthenticationConfig.GetGraphScopes(), authorizationCode).ExecuteAsync();

                // Prepare the access tokens for the current request
                accessTokens = await PrepareAccessTokensAsync(clientApp, authenticationResult.Account, spoRootSiteUrl);

                if (string.IsNullOrEmpty(authenticationResult.AccessToken))
                {
                    throw new Exception("Failed to validate user via Authorization Code");
                }
            }
            catch
            {
                throw new SecurityException("Cannot retrieve valid Access Token and Refresh Token for current user!");
            }

            return accessTokens;
        }

        private async Task<Dictionary<string, string>> PrepareAccessTokensAsync(IConfidentialClientApplication clientApp, IAccount account, string spoRootSiteUrl)
        {
            // Prepare the variable to hold the result
            var accessTokens = new Dictionary<string, string>();

            // Retrieve the Microsoft Graph Access Token
            var graphAccessTokenResult = await clientApp.AcquireTokenSilent(
                AuthenticationConfig.GetGraphScopes(), account).ExecuteAsync().ConfigureAwait(false);
            var graphAccessToken = graphAccessTokenResult.AccessToken;

            // Retrieve the SPO Access Token
            var spoAccessTokenResult = await clientApp.AcquireTokenSilent(
                AuthenticationConfig.GetSpoScopes(spoRootSiteUrl), account).ExecuteAsync().ConfigureAwait(false);
            var spoAccessToken = spoAccessTokenResult.AccessToken;

            // Retrieve the SPO URL for the Admin Site
            var adminSiteUrl = spoRootSiteUrl.Replace(".sharepoint.com", "-admin.sharepoint.com");

            // Retrieve the SPO Access Token
            var spoAdminAccessTokenResult = await clientApp.AcquireTokenSilent(
                AuthenticationConfig.GetSpoScopes(adminSiteUrl), account).ExecuteAsync().ConfigureAwait(false);
            var spoAdminAccessToken = spoAdminAccessTokenResult.AccessToken;

            // Configure the resulting dictionary
            accessTokens.Add(new Uri(AuthenticationConfig.GraphBaseUrl).Authority, graphAccessToken);
            accessTokens.Add(new Uri(spoRootSiteUrl).Authority, spoAccessToken);
            accessTokens.Add(new Uri(adminSiteUrl).Authority, spoAdminAccessToken);

            return accessTokens;
        }

        private static string GetAccountId(ClaimsPrincipal claimsPrincipal)
        {
            string oid = GetObjectId(claimsPrincipal);
            string tid = GetTenantId(claimsPrincipal);
            return $"{oid}.{tid}";
        }

        private static string GetObjectId(ClaimsPrincipal claimsPrincipal)
        {
            return GetClaimValue(claimsPrincipal, Oid, ObjectId);
        }

        private static string GetTenantId(ClaimsPrincipal claimsPrincipal)
        {
            return GetClaimValue(claimsPrincipal, Tid, TenantId);
        }

        private static string GetClaimValue(ClaimsPrincipal claimsPrincipal, params string[] claimNames)
        {
            if (claimsPrincipal == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            for (var i = 0; i < claimNames.Length; i++)
            {
                var currentValue = FindFirstValue(claimsPrincipal, claimNames[i]);
                if (!string.IsNullOrEmpty(currentValue))
                {
                    return currentValue;
                }
            }

            return null;
        }

        private static string FindFirstValue(ClaimsPrincipal claimsPrincipal, string type)
        {
            return claimsPrincipal.FindFirst(type)?.Value;
        }
    }
}
