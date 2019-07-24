using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace SharePointPnP.ProvisioningApp.WebApi.Controllers
{
    [Authorize]
    public class ValuesController : ApiController
    {
        /// <summary>
        /// Provides the whole list of values
        /// </summary>
        /// <remarks>
        /// This is just a fake method to test the overall infrastructure
        /// </remarks>
        /// <returns></returns>
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            // user_impersonation is the default permission exposed by applications in Azure AD
            var scopeClaim = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope");
            var appIdClaim = ClaimsPrincipal.Current.FindFirst("appid");

            if ((scopeClaim == null && appIdClaim == null) ||
                (scopeClaim != null && scopeClaim.Value != "Api.Invoke") ||
                (appIdClaim != null && appIdClaim.Value != "07e256ce-7e72-47aa-80bb-0cc6530fba1a"))
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ReasonPhrase = "The Scope claim does not contain 'Api.Invoke' or scope claim not found"
                });
            }

            return new string[] { "value1", "value2" };
        }

        /// <summary>
        /// Returns a single value
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }
    }
}