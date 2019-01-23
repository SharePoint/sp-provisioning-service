using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharePointPnP.ProvisioningApp.Sync.AzureStorage;
using SharePointPnP.ProvisioningApp.Sync.FileSystem;
using SharePointPnP.ProvisioningApp.Sync.GitHub;
using SharePointPnP.ProvisioningApp.Synchronization;

namespace SharePointPnP.ProvisioningApp.SyncTests
{
    [TestClass]
    public class GitHubTemplateProviderTests
    {
        private GitHubTemplatesProvider _provider;

        [TestInitialize]
        public void Init()
        {
            Uri baseUrl = new Uri(ConfigurationManager.AppSettings["GitHubTemplateProvider:BaseUrl"]);
            string repositoryUrl = ConfigurationManager.AppSettings["GitHubTemplateProvider:RepositoryPath"];
            string personalAccessToken = ConfigurationManager.AppSettings["GitHubTemplateProvider:PersonalAccessToken"];

            _provider = new GitHubTemplatesProvider(baseUrl, repositoryUrl, personalAccessToken);
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
    }
}
