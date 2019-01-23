using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SharePointPnP.ProvisioningApp.Synchronization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SharePointPnP.ProvisioningApp.Sync.AzureStorage
{
    public class BlobTemplatesProvider : ITemplatesProvider
    {

        private readonly CloudBlobContainer _container;

        public BlobTemplatesProvider(string connectionString, string containerName)
        {
            CloudStorageAccount csa;
            if (!CloudStorageAccount.TryParse(connectionString, out csa))
                throw new ArgumentException("Cannot create cloud storage account from given connection string.");

            CloudBlobClient blobClient = csa.CreateCloudBlobClient();
            _container = blobClient.GetContainerReference(containerName);
        }

        public async Task CloneAsync(ITemplatesProvider sourceProvider, Action<string> log)
        {
            IEnumerable<ITemplateItem> items = await sourceProvider.GetAsync("", log);
            await CloneAsync(sourceProvider, "", items, log);
        }

        private async Task CloneAsync(ITemplatesProvider sourceProvider, string path, IEnumerable<ITemplateItem> items, Action<string> log)
        {
            HashSet<ITemplateItem> existingItems = new HashSet<ITemplateItem>(await GetAsync(path, log));

            // Add or update the items
            foreach (ITemplateItem item in items)
            {
                log?.Invoke($"Cloning: {item.Path}");

                // Mark as done
                if (!existingItems.Remove(item))
                {
                    existingItems.RemoveWhere(i => i.Path == System.Uri.EscapeUriString(item.Path));
                }

                if (item is ITemplateFolder folder)
                {

                    // Get the children and clone the entire folder
                    IEnumerable<ITemplateItem> folderItems = await sourceProvider.GetAsync(folder.Path, log);
                    await CloneAsync(sourceProvider, folder.Path, folderItems, log);
                }
                else if (item is ITemplateFile file)
                {
                    using (Stream sourceStream = await file.DownloadAsync())
                    {
                        CloudBlockBlob blob = _container.GetBlockBlobReference(item.Path);
                        await blob.UploadFromStreamAsync(sourceStream);
                    }
                }
            }

            // Remove any additional item
            foreach (ITemplateItem item in existingItems)
            {
                log?.Invoke($"Removing: {item.Path}");

                if (item is ITemplateFolder folder)
                {
                    // To remove a directory we need to iterate through children
                    await DeleteDirectoryAsync(item.Path, log);
                }
                else if (item is ITemplateFile file)
                {
                    CloudBlockBlob blob = _container.GetBlockBlobReference(item.Path);
                    await blob.DeleteAsync();
                }
            }
        }

        private async Task DeleteDirectoryAsync(string path, Action<string> log)
        {
            // Get the children
            IEnumerable<ITemplateItem> items = await GetAsync(path, log);

            foreach (ITemplateItem item in items)
            {
                if (item is ITemplateFolder folder)
                {
                    // To remove a directory we need to iterate through children
                    await DeleteDirectoryAsync(item.Path, log);
                }
                else if (item is ITemplateFile file)
                {
                    CloudBlockBlob blob = _container.GetBlockBlobReference(item.Path);
                    await blob.DeleteAsync();
                }
            }
        }

        public Task<IEnumerable<ITemplateItem>> GetAsync(string path, Action<string> log)
        {
            if (String.IsNullOrWhiteSpace(path) || path == "/") path = "";
            CloudBlobDirectory directory = _container.GetDirectoryReference(path);
            return GetAsync(directory);
        }

        private async Task<IEnumerable<ITemplateItem>> GetAsync(CloudBlobDirectory directory)
        {
            var blobs = await Task.Run(() => directory.ListBlobs());

            return blobs.Select(b =>
            {
                if (b is ICloudBlob cb)
                {
                    return (ITemplateItem)new BlobTemplateItem(cb);
                }
                else
                {
                    return new DirectoryTemplateItem(b);
                }
            });
        }

        private class TemplateItem : ITemplateItem
        {
            private readonly IListBlobItem _item;

            public TemplateItem(IListBlobItem item)
            {
                _item = item;
            }

            public virtual string Path => _item.Uri.AbsolutePath.Substring(_item.Container.Uri.AbsolutePath.Length + 1);

            public override bool Equals(object obj)
            {
                if (obj is ITemplateItem i)
                {
                    return i.Path == Path;
                }
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return Path.GetHashCode();
            }
        }

        private class BlobTemplateItem : TemplateItem, ITemplateFile
        {
            private readonly ICloudBlob _blob;

            public BlobTemplateItem(ICloudBlob blob) : base(blob)
            {
                _blob = blob;
            }

            public Uri DownloadUri => _blob.Uri;

            public async Task<Stream> DownloadAsync()
            {
                var client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(DownloadUri, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStreamAsync(); ;
            }
        }

        private class DirectoryTemplateItem : TemplateItem, ITemplateFolder
        {
            public DirectoryTemplateItem(IListBlobItem item) : base(item)
            {
            }

            public override string Path => base.Path.Substring(0, base.Path.Length - 1);
        }
    }
}
