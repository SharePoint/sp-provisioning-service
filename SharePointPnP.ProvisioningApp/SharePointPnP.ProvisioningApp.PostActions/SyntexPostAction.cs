//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using Newtonsoft.Json;
using PnP.Framework;
using PnP.Framework.Http;
using SharePointPnP.ProvisioningApp.Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.PostActions
{
    public class SyntexPostAction : IProvisioningPostAction
    {
        public string Name { get => this.GetType().Name; }

        public async Task Execute(Dictionary<string, string> accessTokens, string targetSiteUrl, string jsonConfiguration = null)
        {
            // Prepare the AuthenticationManager to access the target environment
            AuthenticationManager authManager = new AuthenticationManager();

            // Retrieve the SPO Access Token for SPO
            var spoAuthority = new Uri(targetSiteUrl).Authority;
            var spoAccessToken = accessTokens.ContainsKey(spoAuthority) ? accessTokens[spoAuthority] : null;

            if (spoAccessToken != null)
            {
                // Connect to the root site collection
                using (var clientContext = authManager.GetAccessTokenContext(targetSiteUrl, spoAccessToken))
                {
                    var httpClient = PnPHttpClient.Instance.GetHttpClient(clientContext);

                    var requestUrl = $"{targetSiteUrl.TrimEnd('/')}/_api/machinelearning/models/SetupPrimedLibrary";

                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl))
                    {
                        request.Headers.Add("accept", "application/json;odata=nometadata");

                        PnPHttpClient.AuthenticateRequestAsync(request, clientContext).GetAwaiter().GetResult();

                        var ContentType = "application/json";
                        var contentString = "{'primedLibraryName':'Sample contracts library','packageName':'ContractsSitePackage.cmp'}";
                        request.Content = new StringContent(contentString, System.Text.Encoding.UTF8);
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(ContentType);

                        HttpResponseMessage response = await httpClient.SendAsync(request, new System.Threading.CancellationToken());

                        if (!response.IsSuccessStatusCode)
                        {
                            // Something went wrong...
                            throw new Exception(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        }
                    }
                }
            }
        }
    }
}
