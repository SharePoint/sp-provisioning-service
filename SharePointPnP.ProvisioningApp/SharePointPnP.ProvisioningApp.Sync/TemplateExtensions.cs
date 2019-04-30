//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Synchronization
{
    public static class TemplateExtensions
    {

        public static string GetDirectoryPath(this ITemplateFile file)
        {
            return Path.GetDirectoryName(file.Path).Replace("\\", "/");
        }

        public static async Task<T> DownloadAsJsonAsync<T>(this ITemplateFile file, T sample)
        {
            string json;
            // Download the file
            using (Stream stream = await file.DownloadAsync())
            {
                json = await new StreamReader(stream).ReadToEndAsync();
            }

            // Deserialize the json
            return JsonConvert.DeserializeAnonymousType(json, sample);
        }

        public static ITemplateFile FindFile(this IEnumerable<ITemplateItem> items, params string[] names)
        {
            return items.OfType<ITemplateFile>()
                   .FirstOrDefault(c => names.Contains(Path.GetFileName(c.Path), StringComparer.OrdinalIgnoreCase));
        }
    }
}
