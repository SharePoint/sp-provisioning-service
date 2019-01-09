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
    public class FileSystemTemplateProviderTests
    {
        private FileSystemTemplatesProvider _provider;

        [TestInitialize]
        public void Init()
        {
            string root = ConfigurationManager.AppSettings["FileSystemTemplatesProvider:Root"];
            _provider = new FileSystemTemplatesProvider(root);
        }

        [TestMethod]
        public async Task GetAsync()
        {
            var items = (await _provider.GetAsync("/")).ToArray();

            foreach (var folder in items.OfType<ITemplateFolder>())
            {
                var folderItems = (await _provider.GetAsync(folder.Path)).ToArray();
            }
        }
    }
}
