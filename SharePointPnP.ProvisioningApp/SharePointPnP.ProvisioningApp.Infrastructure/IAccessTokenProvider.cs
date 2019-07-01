//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
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
        /// Allows to read the refresh token for a specific tenant
        /// </summary>
        /// <param name="keyId">The id to get the refresh token</param>
        /// <returns></returns>
        Task<String> ReadRefreshTokenAsync(String keyId);

        /// <summary>
        /// Allows to write a refresh token
        /// </summary>
        /// <param name="keyId">The Id of the item to retrieve</param>
        /// <param name="refreshTokenValue">The refresh token value</param>
        Task WriteRefreshTokenAsync(String keyId, String refreshTokenValue);

        /// <summary>
        /// Allows to get the access token of a specific tenant for a specific resource url
        /// </summary>
        /// <param name="keyId">The Id of the item to retrieve</param>
        /// <param name="resourceUrl">The resource url</param>
        /// <returns></returns>
        Task<string> GetAccessTokenAsync(String keyId, String resourceUrl);

        /// <summary>
        /// Allows to get the access token of a specific tenant for a specific resource url
        /// </summary>
        /// <param name="keyId">The Id of the item to retrieve</param>
        /// <param name="resourceUrl">The resource url</param>
        /// <param name="clientId">The AAD ClientID of the app to use for token retrieval</param>
        /// <param name="clientSecret">The ADD ClientSecret of the app to use for token retrieval</param>
        /// <param name="appUri">The URI of the app registered in AAD</param>
        /// <returns></returns>
        Task<string> GetAccessTokenAsync(String keyId, String resourceUrl, String clientId, String clientSecret, String appUri);

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
        /// <param name="keyId">The Id of the item to retrieve</param>
        /// <param name="authorizationCode">The Authorization Code for OAuth2 Authorization flow</param>
        /// <param name="tenantId">The ID of the target tenant</param>
        /// <param name="clientId">The AAD ClientID of the app to use for token retrieval</param>
        /// <param name="clientSecret">The ADD ClientSecret of the app to use for token retrieval</param>
        /// <param name="resourceUri">The target resource URI</param>
        /// <param name="redirectUri">The redirect URI for the challenge</param>
        /// <returns>Completion of the requested activity</returns>
        Task SetupSecurityFromAuthorizationCodeAsync(String keyId, String authorizationCode, String tenantId, String clientId, String clientSecret, String resourceUri, String redirectUri);
    }
}
