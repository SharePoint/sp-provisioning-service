//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using OfficeDevPnP.Core;
using OfficeDevPnP.Core.ALM;
using SharePointPnP.ProvisioningApp.Infrastructure;
using SharePointPnP.ProvisioningApp.Infrastructure.DomainModel.Provisioning;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.ProvisioningPreRequirements
{
    public class LearningPathwaysPreReq : IProvisioningPreRequirement
    {
        private const string MicrosoftCustomLearningSite_PropertyName = "MicrosoftCustomLearningSite";

        public string Name { get => this.GetType().Name; }

        /// <summary>
        /// Checks the requirements about the Learning Pathways 
        /// </summary>
        /// <param name="canProvisionModel">Object with information about the current provisioning</param>
        /// <param name="tokenId">Token ID to retrieve access tokens, in case of need</param>
        /// <returns>True if the pre-requirement is fullfilled, or false otherwise</returns>
        /// <remarks>
        /// This method checks if the Microsoft Learning Pathways portal is installed in the target tenant, and if not the pre-requirement is not satisfied
        /// Applies the following checks:
        /// 1) Existence of the Tenant Property called 'MicrosoftCustomLearningSite'
        /// 2) Existence of the site at the URL declared in the 'MicrosoftCustomLearningSite' property
        /// </remarks>
        public async Task<bool> Validate(CanProvisionModel canProvisionModel, string tokenId, string jsonConfiguration = null)
        {
            // Load the configuration, if any or throw an exception
            if (string.IsNullOrEmpty(jsonConfiguration))
            {
                throw new ArgumentNullException(nameof(jsonConfiguration));
            }

            var config = JsonConvert.DeserializeAnonymousType(jsonConfiguration, new
            {
                minVersion = 0,
            });

            // Prepare the AuthenticationManager to access the target environment
            AuthenticationManager authManager = new AuthenticationManager();

            // Retrieve the SPO URL for the Admin Site
            var rootSiteUrl = canProvisionModel.SPORootSiteUrl;

            // Retrieve the SPO Access Token for SPO
            var spoAccessToken = await ProvisioningAppManager.AccessTokenProvider.GetAccessTokenAsync(
                tokenId, rootSiteUrl,
                ConfigurationManager.AppSettings["ida:ClientId"],
                ConfigurationManager.AppSettings["ida:ClientSecret"],
                ConfigurationManager.AppSettings["ida:AppUrl"]);

            // Connect to the root site collection
            using (var clientContext = authManager.GetAzureADAccessTokenAuthenticatedContext(rootSiteUrl, spoAccessToken))
            {
                // Try to read the Tenant Property (Storage Entity) with the URL of the Learning Pathways site                
                var learningPathwaysUrl = clientContext.Web.GetStorageEntity(MicrosoftCustomLearningSite_PropertyName);
                clientContext.Load(learningPathwaysUrl, se => se.Value);
                await clientContext.ExecuteQueryRetryAsync();

                // If the property is null or empty it means that Learning Pathways is not installed
                if (learningPathwaysUrl == null || 
                    learningPathwaysUrl.ServerObjectIsNull() ||
                    string.IsNullOrEmpty(learningPathwaysUrl.Value))
                {
                    return false;
                }
                else
                {
                    try
                    {
                        // Otherwise try to connect to the Learning Pathways site to see if it still exists
                        Uri lpwUrl = new Uri(new Uri(rootSiteUrl), learningPathwaysUrl.Value);
                        using (var lpwContext = authManager.GetAzureADAccessTokenAuthenticatedContext(lpwUrl.AbsoluteUri, spoAccessToken))
                        {
                            var web = lpwContext.Web;
                            lpwContext.Load(web, w => w.Title);
                            await lpwContext.ExecuteQueryRetryAsync();

                            // Check if the Learning Pathways app is installed and is the latest one
                            bool lpAppValid = false;
                            var manager = new AppManager(lpwContext);
                            var siteApps = manager.GetAvailable()?.Where(a => a.InstalledVersion != null)?.ToList();
                            if (siteApps != null && siteApps.Any())
                            {
                                var lpApp = siteApps.FirstOrDefault(p => p.Title.Equals("Microsoft 365 learning pathways", StringComparison.InvariantCultureIgnoreCase));
                                if (lpApp != null)
                                {
                                    if (lpApp.InstalledVersion.Major >= config.minVersion)
                                    {
                                        lpAppValid = true;
                                    }
                                }
                            }



                            if (!lpAppValid)
                            {
                                // Setup can't continue, please install LP or upgrade to the latest version
                                return false;
                            }

                            return true;
                        }
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
        }
    }
}
