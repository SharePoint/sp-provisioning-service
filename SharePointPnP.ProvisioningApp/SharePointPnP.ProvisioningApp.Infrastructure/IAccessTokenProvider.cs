//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Infrastructure
{
    /// <summary>
    /// This interface defines the basic behavior for
    /// an access token provider
    /// </summary>
    public interface IAccessTokenProvider
    {
        /// <summary>
        /// Allows to get the access token of a specific tenant for a specific set of scopes
        /// </summary>
        /// <param name="scopes">The permission scopes to request</param>
        /// <returns></returns>
        Task<string> GetAccessTokenAsync(string[] scopes);

        /// <summary>
        /// Allows to get the access token of a specific tenant for a specific set of scopes
        /// </summary>
        /// <param name="clientId">The AAD ClientID of the app to use for token retrieval</param>
        /// <param name="clientSecret">The ADD ClientSecret of the app to use for token retrieval</param>
        /// <param name="appUri">The URI of the app registered in AAD</param>
        /// <param name="scopes">The permission scopes to request</param>
        /// <returns></returns>
        Task<string> GetAccessTokenAsync(String clientId, String clientSecret, String appUri, string[] scopes);

        /// <summary>
        /// Allows to get the access token of a specific tenant for a specific set of scopes
        /// </summary>
        /// <param name="clientApp">The MSAL Confidential Client App</param>
        /// <param name="scopes">The permission scopes to request</param>
        /// <returns></returns>
        Task<string> GetAccessTokenAsync(IConfidentialClientApplication clientApp, string[] scopes);

        /// <summary>
        /// Allows to get an app-only access token for a specific resource url
        /// </summary>
        /// <param name="resourceUrl">The resource url</param>
        /// <param name="clientId">The AAD ClientID of the app to use for token retrieval</param>
        /// <param name="clientSecret">The ADD ClientSecret of the app to use for token retrieval</param>
        /// <param name="appUri">The URI of the app registered in AAD</param>
        Task<String> GetAppOnlyAccessTokenAsync(String resourceUrl, String tenantId, String clientId, String clientSecret, String appUri);

        /// <summary>
        /// Retrieves a valid Refresh Token using a provided Authorization Code
        /// </summary>
        /// <param name="authorizationCode">The Authorization Code for OAuth2 Authorization flow</param>
        /// <param name="tenantId">The ID of the target tenant</param>
        /// <param name="clientId">The AAD ClientID of the app to use for token retrieval</param>
        /// <param name="clientSecret">The ADD ClientSecret of the app to use for token retrieval</param>
        /// <param name="resourceUri">The target resource URI</param>
        /// <param name="redirectUri">The redirect URI for the challenge</param>
        /// <param name="spoRootSiteUrl">The root URL of SPO for the target tenant</param>
        /// <returns>The Access Tokens for the current session</returns>
        Task<Dictionary<string, string>> SetupSecurityFromAuthorizationCodeAsync(String authorizationCode, String tenantId, String clientId, String clientSecret, String resourceUri, String redirectUri, String spoRootSiteUrl);
    }
}
