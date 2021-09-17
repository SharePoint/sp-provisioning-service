//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using SharePointPnP.ProvisioningApp.Infrastructure.DomainModel.Provisioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SharePointPnP.ProvisioningApp.WebApi.Models
{
    /// <summary>
    /// Defines the contract for staring a new ProvisionContentPack API request
    /// </summary>
    public class ProvisionContentPackRequest : ProvisioningRequest
    {
        /// <summary>
        /// The OAuth2 Authorization Code
        /// </summary>
        public String AuthorizationCode { get; set; }

        /// <summary>
        /// The pre-defined OAuth2 Access Tokens
        /// </summary>
        public Dictionary<string, string> AccessTokens { get; set; }

        /// <summary>
        /// The OAuth2 Redirect URI provided while requesting the Authorization Code
        /// </summary>
        public String RedirectUri { get; set; }

        /// <summary>
        /// The UPN of the requesting user
        /// </summary>
        public String UserPrincipalName { get; set; }

        /// <summary>
        /// The email to use for provisioning notifications
        /// </summary>
        public String NotificationEmail { get; set; }
    }
}