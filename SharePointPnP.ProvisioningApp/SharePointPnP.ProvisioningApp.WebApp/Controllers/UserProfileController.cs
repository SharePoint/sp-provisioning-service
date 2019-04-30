//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SharePointPnP.ProvisioningApp.WebApp.Controllers
{
    public class UserProfileController : ApiController
    {
        [HttpGet()]
        [Route("UserProfile/Username")]
        public String GetUserName()
        {
            String result = String.Empty;

            if (System.Threading.Thread.CurrentPrincipal != null &&
                System.Threading.Thread.CurrentPrincipal.Identity != null &&
                System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                result = System.Threading.Thread.CurrentPrincipal.Identity.Name;
            }

            return (result);
        }
    }
}
