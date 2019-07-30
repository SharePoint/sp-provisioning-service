//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.DomainModel
{
    /// <summary>
    /// Defines a Consumer App for the APIs
    /// </summary>
    public class ConsumerApp
    {
        /// <summary>
        /// The ClientId of the target app
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The display name of the target app
        /// </summary>
        public String DisplayName { get; set; }

        /// <summary>
        /// A reference email address for the app
        /// </summary>
        public String ContactEmail { get; set; }
    }
}
