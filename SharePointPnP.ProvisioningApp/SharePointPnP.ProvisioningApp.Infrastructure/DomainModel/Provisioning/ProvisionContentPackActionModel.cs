//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SharePointPnP.ProvisioningApp.Infrastructure.DomainModel.Provisioning
{
    /// <summary>
    /// Defines a Provision Content Pack Action Model
    /// </summary>
    public class ProvisionContentPackActionModel
    {
        /// <summary>
        /// References the target Office 365 tenant
        /// </summary>
        [DisplayName("Tenant ID")]
        [JsonProperty(PropertyName = "tenantId")]
        public string TenantId { get; set; }

        /// <summary>
        /// References the user who is asking to execute the provisioning action
        /// </summary>
        [DisplayName("UserPrincipalName")]
        [JsonProperty(PropertyName = "upn")]
        public string UserPrincipalName { get; set; }

        /// <summary>
        /// Represents the Package to apply to the target
        /// </summary>
        [DisplayName("Package ID")]
        [JsonProperty(PropertyName = "packageId")]
        public String PackageId { get; set; }

        /// <summary>
        /// Represents the Return Url for the provisioning action
        /// </summary>
        [DisplayName("Return Url")]
        [JsonIgnore]
        public String ReturnUrl { get; set; }
    }
}
