//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using SharePointPnP.ProvisioningApp.Synchronization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Sync.FileSystem
{
    public class FileSystemTemplatesProvider : ITemplatesProvider
    {

        private readonly DirectoryInfo _root;

        private const string SerializerMetadataKey = "serializer";

        public FileSystemTemplatesProvider(string root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));

            _root = new DirectoryInfo(root);
        }

        public Task CloneAsync(ITemplatesProvider sourceProvider, Action<string> log)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ITemplateItem>> GetAsync(string path, Action<string> log)
        {
            if (String.IsNullOrWhiteSpace(path) || path == "/")
            {
                return GetAsync(_root);
            }

            return GetAsync(new DirectoryInfo(Path.Combine(_root.FullName, path)));
        }

        private async Task<IEnumerable<ITemplateItem>> GetAsync(DirectoryInfo directory)
        {
            var items = directory.EnumerateFileSystemInfos();

            return items.Select(b =>
            {
                if (b is FileInfo fi)
                    return (ITemplateItem)new TemplateFile(fi, _root);
                return new TemplateDirectory((DirectoryInfo)b, _root);
            });
        }

        private class TemplateItem : ITemplateItem
        {
            private readonly DirectoryInfo _root;
            private readonly FileSystemInfo _item;

            public TemplateItem(FileSystemInfo item, DirectoryInfo root)
            {
                _root = root;
                _item = item;
            }

            public string Path => new Uri(_item.FullName).ToString().Substring(new Uri(_root.FullName).ToString().Length + 1);

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

        private class TemplateFile : TemplateItem, ITemplateFile
        {
            private readonly FileInfo _file;

            public TemplateFile(FileInfo file, DirectoryInfo root) : base(file, root)
            {
                _file = file;
            }

            public Uri DownloadUri => new Uri(_file.FullName);

            public Task<Stream> DownloadAsync(int retryCount = 10, int delay = 500)
            {
                return Task.FromResult<Stream>(File.OpenRead(DownloadUri.AbsolutePath));
            }
        }

        private class TemplateDirectory : TemplateItem, ITemplateFolder
        {
            public TemplateDirectory(DirectoryInfo directory, DirectoryInfo root) : base(directory, root)
            {

            }
        }
    }
}
