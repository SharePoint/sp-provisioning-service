//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using SharePointPnP.ProvisioningApp.Infrastructure.DomainModel.Provisioning;
using SharePointPnP.ProvisioningApp.Infrastructure.QueueUtilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Infrastructure
{
    /// <summary>
    /// This type is the entry point for all the providers offered
    /// by the Infrastructure project of SharePointPnP.ProvisioningApp
    /// </summary>
    public static class ProvisioningAppManager
    {
        private static readonly Lazy<IAccessTokenProvider> _accessTokenProvider = new Lazy<IAccessTokenProvider>(() => {
            return (ProvisioningAppManager.CreateProviderInstance<IAccessTokenProvider>(ConfigurationManager.AppSettings["SPPA:AccessTokenProvider"] ?? Environment.GetEnvironmentVariable("SPPA:AccessTokenProvider")));
        });

        public static IAccessTokenProvider AccessTokenProvider
        {
            get { return (_accessTokenProvider.Value); }
        }

        //private static readonly Lazy<ISecurityTokensService> _securityTokensServiceProvider = new Lazy<ISecurityTokensService>(() => {
        //    return (ProvisioningAppManager.CreateProviderInstance<ISecurityTokensService>(ConfigurationManager.AppSettings["SPPA:SecurityTokensServiceProvider"]));
        //});

        //public static ISecurityTokensService SecurityTokensServiceProvider
        //{
        //    get { return (_securityTokensServiceProvider.Value); }
        //}

        private static TProvider CreateProviderInstance<TProvider>(String providerTypeName)
            where TProvider : class
        {
            Type providerType = Type.GetType(providerTypeName, true);
            var provider = Activator.CreateInstance(providerType) as TProvider;
            return (provider);
        }

        public static bool IsTestingEnvironment
        {
            // Default value: false
            get { return (Boolean.Parse((ConfigurationManager.AppSettings["TestEnvironment"] ?? Environment.GetEnvironmentVariable("TestEnvironment")) ?? "false")); }
        }

        public static async Task EnqueueProvisioningRequest(ProvisioningActionModel model, String logoFileName = null, Stream logoFile = null)
        {
            if (!String.IsNullOrEmpty(model.MetadataPropertiesJson))
            {
                model.MetadataProperties = JsonConvert.DeserializeObject<Dictionary<String, MetadataProperty>>(model.MetadataPropertiesJson);
            }

            // If there is an input file for the logo
            if (logoFile != null)
            {
                // Generate a random file name
                model.CustomLogo = $"{Guid.NewGuid()}-{logoFileName}";

                // Get a reference to the blob storage account
                var blobLogosConnectionString = ConfigurationManager.AppSettings["BlobLogosProvider:ConnectionString"] ?? Environment.GetEnvironmentVariable("BlobLogosProvider:ConnectionString");
                var blobLogosContainerName = ConfigurationManager.AppSettings["BlobLogosProvider:ContainerName"] ?? Environment.GetEnvironmentVariable("BlobLogosProvider:ConnectionString");

                CloudStorageAccount csaLogos;
                if (!CloudStorageAccount.TryParse(blobLogosConnectionString, out csaLogos))
                    throw new ArgumentException("Cannot create cloud storage account from given connection string.");

                CloudBlobClient blobLogosClient = csaLogos.CreateCloudBlobClient();
                CloudBlobContainer containerLogos = blobLogosClient.GetContainerReference(blobLogosContainerName);

                // Store the file in the blob storage
                CloudBlockBlob blobLogo = containerLogos.GetBlockBlobReference(model.CustomLogo);
                await blobLogo.UploadFromStreamAsync(logoFile);
            }

            var queueTarget = CloudConfigurationManager.GetSetting("SPPA:QueueTarget")?.ToUpper() ?? "BLOB";

            switch (queueTarget)
            {
                case "SERVICEBUS":
                    var sbConnectionString = CloudConfigurationManager.GetSetting("SPPA:ServiceBusConnectionString");
                    var sbQueueName = CloudConfigurationManager.GetSetting("SPPA:ServiceBusQueueName");
                    await ServiceBusQueueUtility.EnqueueMessageAsync(sbConnectionString, sbQueueName, model);
                    break;
                case "BLOB":
                default:
                    var blobConnectionString = CloudConfigurationManager.GetSetting("SPPA:StorageConnectionString");
                    var blobQueueName = CloudConfigurationManager.GetSetting("SPPA:StorageQueueName");
                    await BlobStorageQueueUtility.EnqueueMessageAsync(blobConnectionString, blobQueueName, model);
                    break;
            }
        }
    }
}
