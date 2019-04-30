//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Synchronization
{
    public interface ITemplateItem
    {
        string Path { get; }       
    }

    public interface ITemplateFile : ITemplateItem
    {
        Uri DownloadUri { get; }

        Task<Stream> DownloadAsync();
    }

    public interface IMarkdownFile : ITemplateFile
    {
        Task<string> GetHtmlAsync();
    }

    public interface ITemplateFolder : ITemplateItem
    {

    }
    
}
