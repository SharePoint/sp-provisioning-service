//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
//
using Newtonsoft.Json;
using SharePointPnP.ProvisioningApp.Sync.GitHub.ApiModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Sync.GitHub
{
    public class GitHubHelper
    {
        private const string CONTENTS_PATH = "contents";
        private const string COMMITS_PATH = "commits";
        private const string USERS_PATH = "users";
        private const string RATE_LIMIT_PATH = "rate_limit";
        private const string PATH = "path";

        private readonly string _repositoryUrl;
        private readonly Uri _baseUrl;
        private readonly string _personalAccessToken;

        private HttpClient _httpClient;


        public GitHubHelper(Uri baseUrl, string repositoryPath, string personalAccessToken)
        {
            _repositoryUrl = repositoryPath ?? throw new ArgumentNullException(nameof(repositoryPath));
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _personalAccessToken = personalAccessToken ?? throw new ArgumentNullException(nameof(personalAccessToken));

            _httpClient = GetNewHttpClient();
        }

        public async Task<RateLimitResponse> GetRateLimitAsync()
        {
            string url = $"{_baseUrl}{RATE_LIMIT_PATH}";
            return await MakeRequestAsync<RateLimitResponse>(url);
        }

        /// <summary>
        /// Get the contents of the root folder
        /// </summary>
        /// <param name="filterPath">If specified filter the content by the specified path</param>
        /// <returns>The folder content</returns>
        public async Task<ContentResponse[]> GetContentsAsync(String filterPath = null)
        {
            // https://api.github.com/repos/OfficeDev/PnP/contents[/filterPath]?ref=master
            // filterPath example: Samples/AzureAD.GroupAuthorization/AzureAD.GroupAuthorization/Utils/GraphUtil.cs
            // ?ref=master can be used to specify the branch

            Uri url = new Uri(_baseUrl, $"{_repositoryUrl}/{CONTENTS_PATH}{(!String.IsNullOrWhiteSpace(filterPath) ? "/" + filterPath : String.Empty)}");
            return await MakeRequestAsync<ContentResponse[]>(url.ToString());
        }        

        /// <summary>
        /// Get the content of a specific file
        /// </summary>
        /// <param name="fileUrl">The url of the file</param>
        /// <returns>The content as string of the specified file</returns>
        public async Task<Stream> GetFileContentAsync(String fileUrl
            //, bool html
            )
        {
            int retries = 0;
            while (true)
            {
                try
                {
                    //// Get only the file path relative to the repository
                    //fileUrl = fileUrl.ToLower().Replace(SettingsHelper.GitHubRepositoryUrl.ToLower(), String.Empty);

                    //// Remove the "contents" string
                    //fileUrl = fileUrl.Substring(CONTENTS_PATH.Length);

                    //// Concatenate the raw repository url with the file path
                    //fileUrl = $"{SettingsHelper.GitHubRawRepositoryUrl}{(fileUrl.StartsWith("/") ? fileUrl.Substring(1) : fileUrl)}";

                    //// Get the complete file url with query credentials
                    //string url = $"{fileUrl}{(fileUrl.Contains("?") ? "&" : "?")}{GetQueryStringCredentials()}";
                    string url = $"{fileUrl}{(fileUrl.Contains("?") ? "&" : "?")}";

                    // No more supported/working
                    //if (html) client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.html");

                    // Get the file
                    var response = await _httpClient.GetAsync(url);

                    // Read file content as string
                    return await response.Content.ReadAsStreamAsync();
                }
                catch when (retries++ < 3) // Retry
                {

                }
            }
        }

        /// <summary>
        /// Get the commits filtered by path
        /// </summary>
        /// <param name="filterPath">The path to filter</param>
        /// <returns>The commits of the specified path</returns>
        public async Task<CommitResponse[]> GetCommitsAsync(String filterPath)
        {
            Uri url = new Uri(_baseUrl, $"{_repositoryUrl}/{COMMITS_PATH}?{PATH}={filterPath}");
            return await MakeRequestAsync<CommitResponse[]>(url.ToString());
        }

        /// <summary>
        /// Get the author
        /// </summary>
        /// <param name="username">The username of the author</param>
        /// <returns>The author with the specified username</returns>
        public async Task<User> GetAuthorAsync(String username)
        {
            string url = $"{_baseUrl}{USERS_PATH}/{username}";
            return await MakeRequestAsync<User>(url);
        }

        /// <summary>
        /// Get a new HttpClient with already the Header User-Agent set
        /// </summary>
        /// <returns></returns>
        public HttpClient GetNewHttpClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "PnP Provisioning Service");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("token", _personalAccessToken);

            return client;
        }

        /// <summary>
        /// Make an async request with HttpClient
        /// </summary>
        /// <typeparam name="T">The Type of the response</typeparam>
        /// <param name="url">The url to get the value from</param>
        /// <param name="retryCount">The number of retries</param>
        /// <param name="delay">The delay between retries, with exponential back-off</param>
        /// <returns>The object returned from the request</returns>
        private async Task<T> MakeRequestAsync<T>(string url, int retryCount = 10, int delay = 500)
        {
            try
            {
                for (Int32 c = 0; c < retryCount; c++)
                {
                    var response = await _httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        JsonSerializerSettings settings = new JsonSerializerSettings();
                        settings.NullValueHandling = NullValueHandling.Ignore;
                        return JsonConvert.DeserializeObject<T>(json, settings);
                    }

                    // Exponential back-off
                    await Task.Delay(delay * retryCount);
                }
            }
            catch (Exception e)
            {
                // NOOP here
            }

            return default(T);
        }

        /// <summary>
        /// Convert markdown to html
        /// </summary>
        /// <param name="markdown">The markdown</param>
        /// <returns>The HTML</returns>
        public async Task<string> GetHtmlFromMarkdownAsync(String markdown)
        {
            var url = $"{_baseUrl}markdown";

            int retries = 0;
            while (true)
            {
                try
                {
                    string json = JsonConvert.SerializeObject(new
                    {
                        text = markdown
                    });
                    var response = await _httpClient.PostAsync(url, new StringContent(json)
                    {
                        Headers =
                        {
                            ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json")
                        }
                    });
                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadAsStringAsync();
                }
                catch when (retries++ < 3) // Retry
                {

                }
            }
        }
    }
}
