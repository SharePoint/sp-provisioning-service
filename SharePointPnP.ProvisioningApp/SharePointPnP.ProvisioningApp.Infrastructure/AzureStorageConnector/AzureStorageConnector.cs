using Azure;
using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using PnP.Framework.Provisioning.Connectors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharePointPnP.ProvisioningApp.Infrastructure
{
    /// <summary>
    /// Connector for files in Azure blob storage
    /// </summary>
    public class AzureStorageConnector : FileConnectorBase
    {
        #region private variables
        private bool initialized = false;
        private BlobServiceClient blobServiceClient = null;
        private const string STORAGEACCOUNT = "StorageAccount";
        private const string TOKENCREDENTIAL = "TokenCredential";
        #endregion

        #region Constructor
        /// <summary>
        /// Base constructor
        /// </summary>
        public AzureStorageConnector() : base()
        {

        }

        /// <summary>
        /// AzureStorageConnector constructor. Allows to directly set Azure Storage name and container, and uses Azure.Identity to authenticate.
        /// </summary>
        /// <param name="credential">Credential (from Azure.Identity) that has access to the Storage Account</param>
        /// <param name="storageAccountName">Name of the Azure container to operate against</param>
        /// <param name="containerName">Name of the Azure container to operate against</param>
        public AzureStorageConnector(TokenCredential credential, string storageAccountName, string containerName) : base()
        {
            if (credential == null)
            {
                throw new ArgumentException("credential");
            }
            if (string.IsNullOrEmpty(storageAccountName))
            {
                throw new ArgumentException("storageAccountName");
            }
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException("containerName");
            }

            containerName = containerName.Replace('\\', '/');

            this.AddParameter(TOKENCREDENTIAL, credential);
            this.AddParameterAsString(STORAGEACCOUNT, storageAccountName);
            this.AddParameterAsString(CONTAINER, containerName);
        }

        /// <summary>
        /// AzureStorageConnector constructor. Allows to directly set Azure Storage key and container
        /// </summary>
        /// <param name="connectionString">Azure Storage Key (DefaultEndpointsProtocol=https;AccountName=yyyy;AccountKey=xxxx)</param>
        /// <param name="containerName">Name of the Azure container to operate against</param>
        public AzureStorageConnector(string connectionString, string containerName) : base()
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("connectionString");
            }
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException("containerName");
            }

            containerName = containerName.Replace('\\', '/');

            this.AddParameterAsString(CONNECTIONSTRING, connectionString);
            this.AddParameterAsString(CONTAINER, containerName);
        }
        #endregion

        #region Base class overrides
        /// <summary>
        /// Get the files available in the default container
        /// </summary>
        /// <returns>List of files</returns>
        public override List<string> GetFiles()
        {
            return GetFiles(GetContainer());
        }

        /// <summary>
        /// Get the files available in the specified container
        /// </summary>
        /// <param name="container">Name of the container to get the files from</param>
        /// <returns>List of files</returns>
        public override List<string> GetFiles(string container)
        {
            if (String.IsNullOrEmpty(container))
            {
                throw new ArgumentException("container");
            }
            container = container.Replace('\\', '/');

            if (!initialized)
            {
                Initialize();
            }

            List<string> result = new List<string>();

            var containerTuple = ParseContainer(container);

            container = containerTuple.Item1;
            string prefix = string.IsNullOrEmpty(containerTuple.Item2) ? null : containerTuple.Item2;

            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(container);

            string continuationToken = null;

            try
            {
                do
                {
                    var resultSegment = blobContainerClient.GetBlobsByHierarchy(prefix: prefix, delimiter: "/")
                        .AsPages(continuationToken);

                    foreach (Page<BlobHierarchyItem> blobPage in resultSegment)
                    {
                        foreach (BlobHierarchyItem blobhierarchyItem in blobPage.Values)
                        {
                            if (!blobhierarchyItem.IsPrefix)
                            {
                                result.Add(blobhierarchyItem.Blob.Name);
                            }
                        }
                        continuationToken = blobPage.ContinuationToken;
                    }
                } while (continuationToken != "");
            }
            catch (RequestFailedException)
            {
                throw;
            }

            return result;
        }

        /// <summary>
        /// Get the folders of the default container
        /// </summary>
        /// <returns>List of folders</returns>
        public override List<string> GetFolders()
        {
            return GetFolders(GetContainer());
        }

        /// <summary>
        /// Get the folders of a specified container
        /// </summary>
        /// <param name="container">Name of the container to get the folders from</param>
        /// <returns>List of folders</returns>
        public override List<string> GetFolders(string container)
        {
            if (String.IsNullOrEmpty(container))
            {
                throw new ArgumentException("container");
            }
            container = container.Replace('\\', '/');

            if (!initialized)
            {
                Initialize();
            }

            List<string> result = new List<string>();
            var containerTuple = ParseContainer(container);

            container = containerTuple.Item1;
            string prefix = string.IsNullOrEmpty(containerTuple.Item2) ? null : containerTuple.Item2;

            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(container);

            string continuationToken = null;

            try
            {
                do
                {
                    var resultSegment = blobContainerClient.GetBlobsByHierarchy(prefix: prefix, delimiter: "/")
                        .AsPages(continuationToken);

                    foreach (Page<BlobHierarchyItem> blobPage in resultSegment)
                    {
                        foreach (BlobHierarchyItem blobhierarchyItem in blobPage.Values)
                        {
                            if (blobhierarchyItem.IsPrefix)
                            {
                                result.Add(blobhierarchyItem.Prefix);
                            }
                        }
                        continuationToken = blobPage.ContinuationToken;
                    }
                } while (continuationToken != "");
            }
            catch (RequestFailedException)
            {
                throw;
            }

            return result;
        }

        /// <summary>
        /// Gets a file as string from the default container
        /// </summary>
        /// <param name="fileName">Name of the file to get</param>
        /// <returns>String containing the file contents</returns>
        public override string GetFile(string fileName)
        {
            return GetFile(fileName, GetContainer());
        }

        /// <summary>
        /// Gets a file as string from the specified container
        /// </summary>
        /// <param name="fileName">Name of the file to get</param>
        /// <param name="container">Name of the container to get the file from</param>
        /// <returns>String containing the file contents</returns>
        public override String GetFile(string fileName, string container)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("fileName");
            }

            if (String.IsNullOrEmpty(container))
            {
                throw new ArgumentException("container");
            }
            container = container.Replace('\\', '/');

            string result = null;
            MemoryStream stream = null;
            try
            {
                stream = GetFileFromStorage(fileName, container);

                if (stream == null)
                {
                    return null;
                }

                result = Encoding.UTF8.GetString(stream.ToArray());
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// Gets a file as stream from the default container
        /// </summary>
        /// <param name="fileName">Name of the file to get</param>
        /// <returns>String containing the file contents</returns>
        public override Stream GetFileStream(string fileName)
        {
            return GetFileStream(fileName, GetContainer());
        }

        /// <summary>
        /// Gets a file as stream from the specified container
        /// </summary>
        /// <param name="fileName">Name of the file to get</param>
        /// <param name="container">Name of the container to get the file from</param>
        /// <returns>String containing the file contents</returns>
        public override Stream GetFileStream(string fileName, string container)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("fileName");
            }

            if (String.IsNullOrEmpty(container))
            {
                throw new ArgumentException("container");
            }
            container = container.Replace('\\', '/');

            return GetFileFromStorage(fileName, container);
        }

        /// <summary>
        /// Saves a stream to the default container with the given name. If the file exists it will be overwritten
        /// </summary>
        /// <param name="fileName">Name of the file to save</param>
        /// <param name="stream">Stream containing the file contents</param>
        public override void SaveFileStream(string fileName, Stream stream)
        {
            SaveFileStream(fileName, GetContainer(), stream);
        }

        /// <summary>
        /// Saves a stream to the specified container with the given name. If the file exists it will be overwritten
        /// </summary>
        /// <param name="fileName">Name of the file to save</param>
        /// <param name="container">Name of the container to save the file to</param>
        /// <param name="stream">Stream containing the file contents</param>
        public override void SaveFileStream(string fileName, string container, Stream stream)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException(nameof(fileName));
            }

            if (String.IsNullOrEmpty(container))
            {
                throw new ArgumentException(nameof(container));
            }
            container = container.Replace('\\', '/');

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!initialized)
            {
                Initialize();
            }

            try
            {
                var containerTuple = ParseContainer(container);

                container = containerTuple.Item1;
                fileName = string.Concat(containerTuple.Item2, fileName);

                BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(container);
                blobContainerClient.CreateIfNotExists(PublicAccessType.None);

                BlobClient blob = blobContainerClient.GetBlobClient(fileName);
                blob.Upload(stream);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Deletes a file from the default container
        /// </summary>
        /// <param name="fileName">Name of the file to delete</param>
        public override void DeleteFile(string fileName)
        {
            DeleteFile(fileName, GetContainer());
        }

        /// <summary>
        /// Deletes a file from the specified container
        /// </summary>
        /// <param name="fileName">Name of the file to delete</param>
        /// <param name="container">Name of the container to delete the file from</param>
        public override void DeleteFile(string fileName, string container)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("fileName");
            }

            if (String.IsNullOrEmpty(container))
            {
                throw new ArgumentException("container");
            }
            container = container.Replace('\\', '/');

            if (!initialized)
            {
                Initialize();
            }

            try
            {
                var containerTuple = ParseContainer(container);

                container = containerTuple.Item1;
                fileName = string.Concat(containerTuple.Item2, fileName);

                BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(container);
                BlobClient blob = blobContainerClient.GetBlobClient(fileName);

                if (blob.Exists())
                {
                    blob.Delete();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Returns a filename without a path
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <returns>Returns filename without path</returns>
        public override string GetFilenamePart(string fileName)
        {
            return Path.GetFileName(fileName);
        }
        #endregion

        #region Private methods
        private void Initialize()
        {
            try
            {
                if (IsConnectionStringAuthentication())
                {
                    blobServiceClient = new BlobServiceClient(GetConnectionString());
                }
                else
                {
                    blobServiceClient = new BlobServiceClient(
                        new Uri($"https://{GetStorageAccount()}.blob.core.windows.net"),
                        GetTokenCredential()
                    );
                }

                initialized = true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool IsConnectionStringAuthentication()
        {
            return this.Parameters.ContainsKey(CONNECTIONSTRING);
        }

        private string GetConnectionString()
        {
            if (IsConnectionStringAuthentication())
            {
                return this.Parameters[CONNECTIONSTRING].ToString();
            }
            else
            {
                throw new Exception("No connection string specified");
            }
        }

        private TokenCredential GetTokenCredential()
        {
            if (this.Parameters.ContainsKey(key: TOKENCREDENTIAL) && this.Parameters[key: TOKENCREDENTIAL] is TokenCredential tc)
            {
                return tc;
            }
            else
            {
                throw new Exception("No tokencredential specified");
            }
        }

        private string GetStorageAccount()
        {
            if (this.Parameters.ContainsKey(key: STORAGEACCOUNT))
            {
                return this.Parameters[key: STORAGEACCOUNT].ToString();
            }
            else
            {
                throw new Exception("No storage account string specified");
            }
        }

        private string GetContainer()
        {
            if (this.Parameters.ContainsKey(key: CONTAINER))
            {
                return this.Parameters[key: CONTAINER].ToString();
            }
            else
            {
                throw new Exception("No container string specified");
            }
        }

        private MemoryStream GetFileFromStorage(string fileName, string container)
        {
            if (!initialized)
            {
                Initialize();
            }

            try
            {
                var containerTuple = ParseContainer(container);

                container = containerTuple.Item1;
                fileName = string.Concat(containerTuple.Item2, fileName);

                BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(container);
                BlobClient blob = blobContainerClient.GetBlobClient(fileName);

                MemoryStream result = new MemoryStream();
                Response<BlobDownloadInfo> download = blob.Download();
                download.Value.Content.CopyTo(result);
                result.Position = 0;

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private Tuple<string, string> ParseContainer(string container)
        {
            var firstOccouranceOfSlash = container.IndexOf('/');
            var folder = string.Empty;

            if (firstOccouranceOfSlash > -1)
            {
                var orgContainer = container;
                container = orgContainer.Substring(0, firstOccouranceOfSlash);
                folder = orgContainer.Substring(firstOccouranceOfSlash + 1);
                if (!folder.Substring(folder.Length - 1, 1).Equals("/", StringComparison.InvariantCultureIgnoreCase))
                {
                    folder += "/";
                }
            }

            return Tuple.Create(container, folder);
        }
        #endregion
    }
}