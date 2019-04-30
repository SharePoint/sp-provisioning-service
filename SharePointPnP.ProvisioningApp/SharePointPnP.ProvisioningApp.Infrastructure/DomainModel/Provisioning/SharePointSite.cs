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
    public class SharePointSite
    {
        public String Id { get; set; }

        public String DisplayName { get; set; }

        public String Name { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime LastModifiedDateTime { get; set; }

        public String WebUrl { get; set; }
    }
}
