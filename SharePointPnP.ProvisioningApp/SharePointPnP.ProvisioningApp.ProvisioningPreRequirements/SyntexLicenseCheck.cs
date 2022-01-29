//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using PnP.Core.Services;
using PnP.Framework;
using PnP.Framework.ALM;
using SharePointPnP.ProvisioningApp.Infrastructure;
using SharePointPnP.ProvisioningApp.Infrastructure.DomainModel.Provisioning;
using SharePointPnP.ProvisioningApp.Infrastructure.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.ProvisioningPreRequirements
{
    public class SyntexLicenseCheck : IProvisioningPreRequirement
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public string Name { get => this.GetType().Name; }

        /// <summary>
        /// Checks the requirements about the Syntex License
        /// </summary>
        /// <param name="canProvisionModel">Object with information about the current provisioning</param>
        /// <returns>True if the pre-requirement is fullfilled, or false otherwise</returns>
        /// <remarks>
        /// This method checks if there is proper Syntex license in the current tenant
        /// </remarks>
        public async Task<bool> Validate(CanProvisionModel canProvisionModel, string jsonConfiguration = null)
        {
            // Prepare the result variable
            var result = false;

            // Load the configuration, if any or throw an exception
            if (string.IsNullOrEmpty(jsonConfiguration))
            {
                throw new ArgumentNullException(nameof(jsonConfiguration));
            }

            var config = JsonConvert.DeserializeAnonymousType(jsonConfiguration, new
            {
                tenantLicense = false,
                userLicense = false,
            });

            // Prepare the AuthenticationManager to access the target environment
            AuthenticationManager authManager = new AuthenticationManager();

            // Retrieve the SPO URL for the Admin Site
            var rootSiteUrl = canProvisionModel.SPORootSiteUrl;

            // Retrieve the SPO Access Token for SPO
            var spoAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                AuthenticationConfig.ClientId,
                AuthenticationConfig.ClientSecret,
                AuthenticationConfig.RedirectUri,
                AuthenticationConfig.GetSpoScopes(rootSiteUrl));

            try
            {
                // Syntex is enabled at tenant level?
                var syntexTenantLicense = await IsSyntexEnabledAsync(rootSiteUrl, spoAccessToken);

                // Syntex is enabled at user level?
                var syntexUserLicense = await IsSyntexEnabledForCurrentUserAsync(rootSiteUrl, spoAccessToken);

                result = (!config.tenantLicense || (config.tenantLicense && syntexTenantLicense))
                    && (!config.userLicense || (config.userLicense && syntexUserLicense));
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        private async Task<bool> IsSyntexEnabledAsync(string rootSiteUrl, string spoAccessToken)
        {
            return await InvokeSyntexApiAsync(
                $"{rootSiteUrl.TrimEnd('/')}/_api/machinelearning/MachineLearningEnabled/MachineLearningCaptureEnabled",
                spoAccessToken);
        }

        private async Task<bool> IsSyntexEnabledForCurrentUserAsync(string rootSiteUrl, string spoAccessToken)
        {
            return await InvokeSyntexApiAsync(
                $"{rootSiteUrl.TrimEnd('/')}/_api/machinelearning/MachineLearningEnabled/UserSyntexEnabled",
                spoAccessToken);
        }

        private async Task<bool> InvokeSyntexApiAsync(string apiUrl, string accessToken)
        {
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json;odata=nometadata");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await httpClient.GetAsync(apiUrl);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return false;
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer
                    .Deserialize<JsonElement>(json).GetProperty("value");

                return result.GetBoolean();
            }
        }
    }
}
