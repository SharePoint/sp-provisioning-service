//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SharePointPnP.ProvisioningApp.WebApp.Models
{
    public class HeaderDataViewModel
    {
        public string SiteTitle { get; set; }

        public string RootSiteUrl { get; set; }

        public bool RenderProvisioningService { get; set; }
    }
}