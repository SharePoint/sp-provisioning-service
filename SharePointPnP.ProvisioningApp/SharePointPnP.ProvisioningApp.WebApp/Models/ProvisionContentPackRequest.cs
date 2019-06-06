using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SharePointPnP.ProvisioningApp.WebApp.Models
{
    /// <summary>
    /// Defines the contract for staring a new ProvisionContentPack API request
    /// </summary>
    public class ProvisionContentPackRequest
    {
        /// <summary>
        /// The OAuth2 Authorization Code
        /// </summary>
        public String AuthorizationCode { get; set; }

        /// <summary>
        /// The target tenant ID
        /// </summary>
        public String TenantId { get; set; }

        /// <summary>
        /// The URL of the SPO root site
        /// </summary>
        public String SPORootSiteUrl { get; set; }

        /// <summary>
        /// The UPN of the requesting user
        /// </summary>
        public String UserPrincipalName { get; set; }

        /// <summary>
        /// The ID of the Package to provision
        /// </summary>
        public String PackageId { get; set; }

        /// <summary>
        /// Any parameters for the provisioning package
        /// </summary>
        public Dictionary<String, String> Parameters { get; set; }
    }
}