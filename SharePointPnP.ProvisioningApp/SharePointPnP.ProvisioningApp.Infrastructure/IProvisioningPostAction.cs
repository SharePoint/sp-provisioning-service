//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using SharePointPnP.ProvisioningApp.Infrastructure.DomainModel.Provisioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Infrastructure
{
    /// <summary>
    /// Interface to define the contract of any Provisioning Post-Action
    /// </summary>
    public interface IProvisioningPostAction
    {
        /// <summary>
        /// The Name of the Provisioning Post-Action (read-only)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Method to execute the Post-Action
        /// </summary>
        /// <param name="jsonConfiguration">An optional JSON string with additional configuration settings for the validation rule</param>
        Task Execute(Dictionary<string, string> accessTokens, string targetSiteUrl, string jsonConfiguration = null);
    }
}
