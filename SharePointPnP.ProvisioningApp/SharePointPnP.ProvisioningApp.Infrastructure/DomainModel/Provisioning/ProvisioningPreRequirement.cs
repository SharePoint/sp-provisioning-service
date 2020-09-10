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
    /// Defines a type to invoke in order to check Pre-Requirements for a provisioning activity
    /// </summary>
    public class ProvisioningPreRequirement
    {
        /// <summary>
        /// The name of the assembly to load the PreRequirement type from
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// The name of the type to load the PreRequirement type from
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// The configuration of the PreRequirement
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// Text of the Pre-Requirement, in case of need
        /// </summary>
        public string PreRequirementContent { get; set; }
    }
}
