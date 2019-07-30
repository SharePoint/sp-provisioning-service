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
    /// Defines the contract for staring a new Provisioning API request
    /// </summary>
    public class ProvisioningRequest
    {
        /// <summary>
        /// The target tenant ID
        /// </summary>
        public String TenantId { get; set; }

        /// <summary>
        /// The URL of the SPO root site
        /// </summary>
        public String SPORootSiteUrl { get; set; }

        /// <summary>
        /// The list of packages to provision
        /// </summary>
        public List<ProvisioningItemModel> Packages { get; set; }

        /// <summary>
        /// Optional list of Webhooks to call during the whole provisioning
        /// </summary>
        public List<ProvisioningWebhook> Webhooks { get; set; }
    }
}