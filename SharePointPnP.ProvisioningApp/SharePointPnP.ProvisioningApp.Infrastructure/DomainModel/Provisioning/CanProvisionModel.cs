//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Infrastructure.DomainModel.Provisioning
{
    /// <summary>
    /// Defines the properties for validating if a package can be provisioned on the target tenant
    /// </summary>
    public class CanProvisionModel
    {
        /// <summary>
        /// Represents the Package to apply to the target
        /// </summary>
        public String PackageId { get; set; }

        /// <summary>
        /// References the target Office 365 tenant
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// References the user who is asking to execute the provisioning action
        /// </summary>
        public string UserPrincipalName { get; set; }

        /// <summary>
        /// Defines whether the current user is SPO Admin or not
        /// </summary>
        public Boolean UserIsSPOAdmin { get; set; }

        /// <summary>
        /// Defines whether the current user is Tenant Global Admin or not
        /// </summary>
        public Boolean UserIsTenantAdmin { get; set; }

        /// <summary>
        /// The URL of the root site collection in the target tenant
        /// </summary>
        public String SPORootSiteUrl { get; set; }

        /// <summary>
        /// Dictionary of OAuth Access Tokens for consuming back-end APIs
        /// </summary>
        public Dictionary<string, string> AccessTokens { get; set; }
    }
}
