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
    /// Interface to define the contract of any Provisioning Pre-Requirement
    /// </summary>
    public interface IProvisioningPreRequirement
    {
        /// <summary>
        /// The Name of the Provisioning Pre-Requirement (read-only)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Method to validate the current Pre-Requirement
        /// </summary>
        /// <param name="canProvisionModel">Object with information about the current provisioning</param>
        /// <param name="jsonConfiguration">An optional JSON string with additional configuration settings for the validation rule</param>
        /// <returns>True if the pre-requirement is fullfilled, or false otherwise</returns>
        Task<bool> Validate(CanProvisionModel canProvisionModel, string jsonConfiguration = null);
    }
}
