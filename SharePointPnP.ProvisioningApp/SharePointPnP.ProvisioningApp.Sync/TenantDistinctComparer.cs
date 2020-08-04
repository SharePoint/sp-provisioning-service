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
    /// <summary>
    /// Compares two tenant items based on the ID only
    /// </summary>
    internal class TenantDistinctComparer : IEqualityComparer<TenantItem>
    {

        public bool Equals(TenantItem x, TenantItem y)
        {
            return x.id == y.id;
        }

        public int GetHashCode(TenantItem obj)
        {
            return obj.id.GetHashCode();
        }
    }
}