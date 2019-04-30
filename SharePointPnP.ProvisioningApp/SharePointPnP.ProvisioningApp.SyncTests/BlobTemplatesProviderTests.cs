//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharePointPnP.ProvisioningApp.Sync.AzureStorage;
using SharePointPnP.ProvisioningApp.Sync.FileSystem;
using SharePointPnP.ProvisioningApp.Synchronization;

namespace SharePointPnP.ProvisioningApp.SyncTests
{
    [TestClass]
    public class BlobTemplatesProviderTests
    {
        private BlobTemplatesProvider _provider;

        [TestInitialize]
        public void Init()
        {
            string connectionString = ConfigurationManager.AppSettings["BlobTemplatesProvider:ConnectionString"];
            string containerName = ConfigurationManager.AppSettings["BlobTemplatesProvider:ContainerName"];
            _provider = new BlobTemplatesProvider(connectionString, containerName);
        }

        [TestMethod]
        public async Task GetAsync()
        {
            var items = (await _provider.GetAsync("/", null)).ToArray();

            foreach (var folder in items.OfType<ITemplateFolder>())
            {
                var folderItems = (await _provider.GetAsync(folder.Path, null)).ToArray();
            }
        }

        [TestMethod]
        public async Task CloneAsync()
        {
            string root = ConfigurationManager.AppSettings["FileSystemTemplatesProvider:Root"];
            var sourceProvider = new FileSystemTemplatesProvider(root);

            await _provider.CloneAsync(sourceProvider, null);
        }
    }
}
