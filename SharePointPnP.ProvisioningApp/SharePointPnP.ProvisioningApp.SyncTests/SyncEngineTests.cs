using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharePointPnP.ProvisioningApp.Sync.AzureStorage;
using SharePointPnP.ProvisioningApp.Sync.GitHub;
using SharePointPnP.ProvisioningApp.Synchronization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Synchronization.Tests
{
    [TestClass()]
    public class SyncEngineTests
    {
        [TestMethod()]
        public async Task RunAsync()
        {
            string connectionString = ConfigurationManager.AppSettings["BlobTemplatesProvider:ConnectionString"];
            string containerName = ConfigurationManager.AppSettings["BlobTemplatesProvider:ContainerName"];
            var cloneProvider = new BlobTemplatesProvider(connectionString, containerName);

            Uri baseUrl = new Uri(ConfigurationManager.AppSettings["GitHubTemplateProvider:BaseUrl"]);
            string repositoryUrl = ConfigurationManager.AppSettings["GitHubTemplateProvider:RepositoryPath"];
            string personalAccessToken = ConfigurationManager.AppSettings["GitHubTemplateProvider:PersonalAccessToken"];

            var sourceProvider = new GitHubTemplatesProvider(baseUrl, repositoryUrl, personalAccessToken);

            var sync = new SyncEngine(sourceProvider, cloneProvider);
            await sync.RunAsync(true);
        }
    }
}