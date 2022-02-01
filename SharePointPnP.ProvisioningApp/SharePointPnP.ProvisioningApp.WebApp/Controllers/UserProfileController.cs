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
    [Authorize]
    public class UserProfileController : ApiController
    {
        [HttpGet()]
        [Route("UserProfile/Username")]
        public String GetUserName()
        {
            String result = String.Empty;

            if (System.Security.Claims.ClaimsPrincipal.Current != null)
            {
                var claim = System.Security.Claims.ClaimsPrincipal.Current.FindFirst("preferred_username");
                if (claim != null)
                {
                    result = claim.Value;
                }
            }

            return (result);
        }
    }
}
