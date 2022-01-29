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
    /// Defines a type to invoke in order to execute the Post-Action for a provisioning activity
    /// </summary>
    public class ProvisioningPostAction
    {
        /// <summary>
        /// The name of the assembly to load the Post-Action type from
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// The name of the type to load the Post-Action type from
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// The configuration of the Post-Action
        /// </summary>
        public string Configuration { get; set; }
    }
}
