//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SharePointPnP.ProvisioningApp.WebApi.Models
{
    /// <summary>
    /// Defines a package parameter
    /// </summary>
    public class PackageParameter
    {
        /// <summary>
        /// The name of the parameter
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// The caption of the parameter
        /// </summary>
        public String Caption { get; set; }

        /// <summary>
        /// The description of the parameter
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// The default value, if any, for the parameter
        /// </summary>
        public String DefaultValue { get; set; }

        /// <summary>
        /// The custom editor control, if any, for the parameter
        /// </summary>
        public String Editor { get; set; }

        /// <summary>
        /// The custom editor settings, if any, for the editor control of the parameter
        /// </summary>
        public String EditorSettings { get; set; }
    }
}