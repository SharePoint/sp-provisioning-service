//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Newtonsoft.Json;
using SharePointPnP.ProvisioningApp.Infrastructure.ADAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Infrastructure.ADAL
{
    public class AccessTokenProvider : IAccessTokenProvider
    {
        private static string aadInstance = EnsureTrailingSlash(ConfigurationManager.AppSettings["ida:AADInstance"]);
        private string authority = aadInstance + "common";

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

        public async Task<String> GetAccessTokenAsync(string keyId, string resourceUri)
        {
            string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
            string clientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
            string appUri = ConfigurationManager.AppSettings["ida:AppUrl"];

            return (await this.GetAccessTokenAsync(keyId, resourceUri, clientId, clientSecret, appUri));
        }

        public async Task<String> GetAccessTokenAsync(string keyId, string resourceUri, String clientId, String clientSecret, String appUri)
        {
            // We need to manually retrieve an AccessToken from a valid RefreshToken
            using (var client = new HttpClient())
            {
                // Prepare the AAD OAuth request URI
                var tokenUri = new Uri($"{authority}/oauth2/token");
                var refreshToken = await this.ReadRefreshTokenAsync(keyId);

                // Prepare the OAuth 2.0 request for an Access Token with Authorization Code
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("redirect_uri", appUri),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("refresh_token", refreshToken),
                    new KeyValuePair<string, string>("resource", resourceUri),
                });

                // Make the HTTP request
                var result = await client.PostAsync(tokenUri, content);
                string jsonToken = await result.Content.ReadAsStringAsync();

                // Get back the OAuth 2.0 response
                var token = JsonConvert.DeserializeObject<OAuthTokenResponse>(jsonToken);

                if (token != null && !String.IsNullOrEmpty(token.RefreshToken))
                {
                    // Save the updated and refreshed RefreshToken
                    await this.WriteRefreshTokenAsync(keyId, token.RefreshToken);
                }

                return (token?.AccessToken);
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

        public async Task<String> ReadRefreshTokenAsync(string keyId)
        {
            var vault = new KeyVaultService();

            // Read any existing properties for the current tenantId
            var properties = await vault.GetAsync(keyId);

            // If any, return the Refresh Token value
            return (properties.ContainsKey("RefreshToken") ? properties["RefreshToken"] : null);
        }

        public async Task WriteRefreshTokenAsync(string keyId, string refreshTokenValue)
        {
            var vault = new KeyVaultService();

            // Read any existing properties for the current tenantId
            var properties = await vault.GetAsync(keyId);

            if (properties == null)
            {
                // If there are no properties, create a new dictionary
                properties = new Dictionary<String, String>();
            }

            // Set/Update the RefreshToken value
            properties["RefreshToken"] = refreshTokenValue;

            // Add or Update the Key Vault accordingly
            await vault.AddOrUpdateAsync(keyId, properties);
        }
    }
}
