//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.KeyVault.WebKey;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Infrastructure
{
    public class KeyVaultService
    {
        private const string SETTINGS_CLIENT_ID = "KeyVault:ClientId";
        private const string SETTINGS_VAULT_ADDRESS = "KeyVault:VaultAddress";
        private const string SETTINGS_CERTIFICATE_THUMBPRINT = "KeyVault:CertificateThumbprint";
        private const string SETTINGS_CERTIFICATE_STORE_NAME = "KeyVault:CertificateStoreName";
        private const string SETTINGS_CERTIFICATE_STORE_LOCATION = "KeyVault:CertificateStoreLocation";

        private KeyVaultClient keyVaultClient;
        private String vaultAddress;

        /// <summary>
        /// Constructor
        /// <para>All parameters are required</para>
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="certificateThumbprint"></param>
        /// <param name="vaultAddress"></param>
        /// <param name="certificateStoreName"></param>
        /// <param name="certificateStoreLocation"></param>
        /// <remarks>All input arguments are required, missing of an input argument result in an ArgumentNullException</remarks>
        public KeyVaultService(string clientId, string certificateThumbprint, string vaultAddress, string certificateStoreName, string certificateStoreLocation)
        {
            #region input arguments validation

            if (String.IsNullOrEmpty(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (String.IsNullOrEmpty(certificateThumbprint))
            {
                throw new ArgumentNullException(nameof(certificateThumbprint));
            }

            if (String.IsNullOrEmpty(vaultAddress))
            {
                throw new ArgumentNullException(nameof(vaultAddress));
            }

            if (String.IsNullOrEmpty(certificateStoreName))
            {
                throw new ArgumentNullException(nameof(certificateStoreName));
            }

            if (String.IsNullOrEmpty(certificateStoreLocation))
            {
                throw new ArgumentNullException(nameof(certificateStoreLocation));
            }

            #endregion

            this.vaultAddress = vaultAddress;

            var certificate = Utilities.FindCertificateByThumbprint(certificateThumbprint, certificateStoreName, certificateStoreLocation);
            var assertionCert = new ClientAssertionCertificate(clientId, certificate);

            keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(
                   (authority, resource, scope) => GetAccessTokenAsync(authority, resource, scope, assertionCert)));
        }

        /// <summary>
        /// Constructor
        /// <para>All settings are loaded from the AppSettings</para>
        /// </summary>
        public KeyVaultService() : this(
            ConfigurationManager.AppSettings[SETTINGS_CLIENT_ID],
            ConfigurationManager.AppSettings[SETTINGS_CERTIFICATE_THUMBPRINT],
            ConfigurationManager.AppSettings[SETTINGS_VAULT_ADDRESS],
            ConfigurationManager.AppSettings[SETTINGS_CERTIFICATE_STORE_NAME],
            ConfigurationManager.AppSettings[SETTINGS_CERTIFICATE_STORE_LOCATION]
            )
        {
        }

        /// <summary>
        /// Adds or update the dictionary values with a specific key to the KeyVault
        /// </summary>
        /// <param name="key">The unique key</param>
        /// <param name="values">The dictionary values to be added</param>
        /// <returns></returns>
        /// <remarks>If an exception is generated the method returns it without any wrap</remarks>
        public async Task AddOrUpdateAsync(string key, IDictionary<String, String> values)
        {
            var tags = new Dictionary<String, String>();

            foreach (var item in values)
            {
                if (!String.IsNullOrEmpty(item.Value) && item.Value.Length > 256)
                {
                    // If the item is longer than 256 split in chunks of 256
                    for (var chunk = 0; chunk <= item.Value.Length / 256; chunk++)
                    {
                        tags.Add($"{item.Key}_{chunk}", item.Value.Substring(chunk * 256,
                            (item.Value.Length - chunk * 256) > 256 ? 256 : item.Value.Length - chunk * 256));
                    }
                }
                else if (!String.IsNullOrEmpty(item.Value))
                {
                    // If the item is shorter than 256 add it directly
                    tags.Add(item.Key, item.Value);
                }
            }

            var attributes = new KeyAttributes(recoveryLevel: "Purgeable");
            attributes.Expires = DateTime.UtcNow.AddHours(12);

            var createdKey = await keyVaultClient.CreateKeyAsync(vaultAddress, key, JsonWebKeyType.Rsa, keyAttributes: attributes, tags: tags);
        }

        /// <summary>
        /// Gets the dictionary values for a specific key
        /// </summary>
        /// <param name="key">The key to search for</param>
        /// <param name="version">The version of the key</param>
        /// <returns>The values for the requested key</returns>
        public async Task<IDictionary<String, String>> GetAsync(string key, string version = null)
        {
            KeyBundle retrievedKey = await GetFullKeyAsync(key, version);

            if (retrievedKey == null)
            {
                return new Dictionary<string, string>();
            }

            var tagsToFix = retrievedKey.Tags
                .Where(t => t.Key.Contains("_"))
                .OrderBy(t => t.Key)
                .Select(t => new
                {
                    Key = t.Key.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries).First(),
                    Tag = t
                })
                .GroupBy(g => g.Key);

            // If there are tags splitted in chunks recompose them
            if (tagsToFix.Count() > 0)
            {
                foreach (var tag in tagsToFix)
                {
                    var finalValue = "";
                    foreach (var v in tag)
                    {
                        finalValue = $"{finalValue}{v.Tag.Value}";
                        retrievedKey.Tags.Remove(v.Tag.Key);
                    }

                    retrievedKey.Tags.Add(tag.Key, finalValue);
                }
            }

            return retrievedKey.Tags;
        }

        /// <summary>
        /// Gets a specific key
        /// </summary>
        /// <param name="key">The key to search for</param>
        /// <param name="version">The version of the key</param>
        /// <returns>The requested key</returns>
        public async Task<KeyBundle> GetFullKeyAsync(string key, string version = null)
        {
            KeyBundle retrievedKey = null;

            try
            {
                if (!String.IsNullOrEmpty(version) || !String.IsNullOrEmpty(key))
                {
                    // If version is specified get the tags for the specific version
                    if (!String.IsNullOrEmpty(version))
                    {
                        retrievedKey = await keyVaultClient.GetKeyAsync(vaultAddress, key, version);
                    }
                    else
                    {
                        // Get the tags for the latest version
                        retrievedKey = await keyVaultClient.GetKeyAsync(vaultAddress, key);
                    }
                }
            }
            catch (KeyVaultErrorException ex)
            {
                if (ex.Body.Error.Code != "KeyNotFound")
                {
                    throw ex;
                }
            }

            return (retrievedKey);
        }


        /// <summary>
        /// Removes a specific key
        /// </summary>
        /// <param name="key">The key to search for</param>
        public async Task RemoveKeyAsync(string key)
        {
            try
            {
                // Deletes a key
                var deletedKey = await keyVaultClient.DeleteKeyAsync(vaultAddress, key);
                // And purges it immediately
                await keyVaultClient.PurgeDeletedKeyAsync(deletedKey.RecoveryId);
            }
            catch (KeyVaultErrorException ex)
            {
                if (ex.Body.Error.Code != "KeyNotFound")
                {
                    throw ex;
                }
            }
        }
        
        /// <summary>
        /// Gets the access token
        /// </summary>
        /// <param name="authority"> Authority </param>
        /// <param name="resource"> Resource </param>
        /// <param name="scope"> scope </param>
        /// <returns> token </returns>
        private async Task<string> GetAccessTokenAsync(string authority, string resource, string scope, ClientAssertionCertificate assertionCert)
        {
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = await context.AcquireTokenAsync(resource, assertionCert).ConfigureAwait(false);

            return result.AccessToken;
        }

        /// <summary>
        /// List all the keys stored in the KeyVault
        /// </summary>
        /// <returns>The list of Keys</returns>
        public async Task<List<String>> ListKeysAsync()
        {
            var result = new List<String>();

            var keysPage = await keyVaultClient.GetKeysAsync(vaultAddress);
            String nextPageLink = null;

            do
            {
                if (!String.IsNullOrEmpty(nextPageLink))
                {
                    keysPage = await keyVaultClient.GetKeysNextAsync(nextPageLink);
                }

                nextPageLink = keysPage.NextPageLink;

                foreach (var key in keysPage)
                {
                    result.Add(key.Identifier.Name);
                }
            } while (!String.IsNullOrEmpty(nextPageLink));


            return (result);
        }
    }
}
