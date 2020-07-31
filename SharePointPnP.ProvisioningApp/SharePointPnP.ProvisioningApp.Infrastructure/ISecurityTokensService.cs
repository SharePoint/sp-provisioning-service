//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Infrastructure
{
    /// <summary>
    /// Interface to define the contract for a Security Tokens service
    /// </summary>
    public interface ISecurityTokensService
    {
        /// <summary>
        /// Adds or update the dictionary values with a specific key
        /// </summary>
        /// <param name="key">The unique key</param>
        /// <param name="values">The dictionary values to be added</param>
        /// <returns></returns>
        /// <remarks>If an exception is generated the method returns it without any wrap</remarks>
        Task AddOrUpdateAsync(string key, IDictionary<String, String> values);

        /// <summary>
        /// Gets the dictionary values for a specific key
        /// </summary>
        /// <param name="key">The key to search for</param>
        /// <param name="version">The version of the key</param>
        /// <returns>The values for the requested key</returns>
        Task<IDictionary<String, String>> GetAsync(string key, string version = null);

        /// <summary>
        /// Removes a specific key
        /// </summary>
        /// <param name="key">The key to search for</param>
        Task RemoveKeyAsync(string key);

        /// <summary>
        /// List all the keys stored in the KeyVault
        /// </summary>
        /// <returns>The list of Keys</returns>
        Task<List<String>> ListKeysAsync();
    }
}
