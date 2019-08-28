//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Infrastructure.DomainModel.Provisioning
{
    /// <summary>
    /// Defines a provisioning item for a ProvisioningActionModel
    /// </summary>
    public class ProvisioningItemModel
    {
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
