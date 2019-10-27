//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using SharePointPnP.ProvisioningApp.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SharePointPnP.ProvisioningApp.WebApp.Models
{
    public class CategoriesMenuViewModel
    {
        public List<Category> Categories { get; set; }

        public String BaseLinksUrl { get; set; }
    }
}