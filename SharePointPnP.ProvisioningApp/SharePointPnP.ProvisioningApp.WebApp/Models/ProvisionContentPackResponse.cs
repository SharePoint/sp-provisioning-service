using OfficeDevPnP.Core.Framework.Provisioning.CanProvisionRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SharePointPnP.ProvisioningApp.WebApp.Models
{
    /// <summary>
    /// Defines the contract for the response of a ProvisionContentPack API request
    /// </summary>
    public class ProvisionContentPackResponse
    {
        /// <summary>
        /// Any CanProvision rule exception
        /// </summary>
        public CanProvisionResult CanProvisionResult { get; set; }

        /// <summary>
        /// Declares whether the provisioning started or not (false by default)
        /// </summary>
        public Boolean ProvisioningStarted { get; set; } = false;
    }
}