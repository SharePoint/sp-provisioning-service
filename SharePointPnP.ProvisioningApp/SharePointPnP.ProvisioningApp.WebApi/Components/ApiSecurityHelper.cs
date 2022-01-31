//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using SharePointPnP.ProvisioningApp.DomainModel;
using SharePointPnP.ProvisioningApp.Infrastructure.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace SharePointPnP.ProvisioningApp.WebApi.Components
{
    /// <summary>
    /// Helper class to manage the security controls (AuthN/AuthZ) for the APIs
    /// </summary>
    public static class ApiSecurityHelper
    {
        /// <summary>
        /// Checks the Authorization for an API request
        /// </summary>
        /// <param name="allowAppOnly">Defines whether an app-only request is allowed or not</param>
        public static void CheckRequestAuthorization(Boolean allowAppOnly)
        {
            // Read the current audience from the configuration
            var audience = AuthenticationConfig.Audience;

            // Read the permission scopes from the configuration
            var scope = AuthenticationConfig.ApiScope;

            // If there isn't a current user identity, there is a security issue
            if (ClaimsPrincipal.Current == null || ClaimsPrincipal.Current.Identity == null)
            {
                ThrowAuthorizationException();
            }

            // Otherwise try to get the permission scope and the appid claims
            var scopeClaim = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope");
            var appIdClaim = ClaimsPrincipal.Current.FindFirst("appid");
            var audienceClaim = ClaimsPrincipal.Current.FindFirst("aud");

            // Check if the audience claim matches the current audience, otherwise there is a security issue
            if (audienceClaim == null ||
                audienceClaim.Value != audience)
            {
                ThrowAuthorizationException();
            }

            // If we have the AppId, check it against the ConsumerApps collection
            if (appIdClaim != null)
            {
                var dbContext = new ProvisioningAppDBContext();
                var consumerAppId = Guid.Parse(appIdClaim.Value);
                var consumerApp = dbContext.ConsumerApps.FirstOrDefault(a => a.Id == consumerAppId);
                if (consumerApp == null)
                {
                    // If the AppId is not "supported", there is a security issue
                    ThrowAuthorizationException();
                }
            }
            else
            {
                // If we don't have the AppId, there is a security issue
                ThrowAuthorizationException();
            }

            // If we're not allowed to accept an app-only request
            if (!allowAppOnly)
            {
                // and there is no scope claim or the scope claim is wrong, there is a security issue
                if (scopeClaim == null ||
                    (scopeClaim != null &&
                    scopeClaim.Value != scope))
                {
                    ThrowAuthorizationException();
                }
            }
        }

        private static void ThrowAuthorizationException()
        {
            throw new HttpResponseException(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                ReasonPhrase = "The current request does not contain a valid Access Token!"
            });
        }
    }
}