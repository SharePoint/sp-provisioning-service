//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Synchronization
{
    internal class TenantItem
    {
        public string id { get; set; }

        public string tenantName { get; set; }

        public string referenceOwner { get; set; }
    }
}
