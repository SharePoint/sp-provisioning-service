using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace SharePointPnP.ProvisioningApp.WebApp.Utils
{
    public static class AuthHelper
    {
        public static (string, string) GetCurrentUserIdentityClaims(ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            string upn = null;
            string tenantId = null;

            var issuer = principal.FindFirst("iss");
            if (issuer != null && !String.IsNullOrEmpty(issuer.Value))
            {
                var issuerValue = issuer.Value;
                if (issuerValue.EndsWith("/v2.0") || issuerValue.EndsWith("/"))
                {
                    // Remove the "/v2.0" part from the issuer URL or the "/"
                    issuerValue = issuerValue.Substring(0, issuerValue.LastIndexOf("/"));
                }
                tenantId = issuerValue.Substring(issuerValue.LastIndexOf("/") + 1);
                upn = System.Security.Claims.ClaimsPrincipal.Current?.FindFirst(ClaimTypes.Upn)?.Value;

                if (string.IsNullOrEmpty(upn))
                {
                    upn = System.Security.Claims.ClaimsPrincipal.Current?.FindFirst("preferred_username")?.Value;
                }
            }

            return (upn, tenantId);
        }
    }
}