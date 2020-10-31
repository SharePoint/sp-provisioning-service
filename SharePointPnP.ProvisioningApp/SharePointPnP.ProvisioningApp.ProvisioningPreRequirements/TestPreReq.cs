//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using SharePointPnP.ProvisioningApp.Infrastructure;
using SharePointPnP.ProvisioningApp.Infrastructure.DomainModel.Provisioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.ProvisioningPreRequirements
{
    public class TestPreReq : IProvisioningPreRequirement
    {
        public string Name { get => this.GetType().Name; }

        public async Task<bool> Validate(CanProvisionModel canProvisionModel, string tokenId, string jsonConfiguration = null)
        {
            return false;
        }
    }
}
