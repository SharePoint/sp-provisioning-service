using SharePointPnP.ProvisioningApp.Sync.GitHub.ApiModels;
using SharePointPnP.ProvisioningApp.Synchronization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SharePointPnP.ProvisioningApp.Sync.GitHub
{
    public class GitHubTemplatesProvider : ITemplatesProvider, IAuthorProvider
    {

        private readonly GitHubHelper _helper;

        public GitHubTemplatesProvider(Uri baseUrl, string repositoryPath, string personalAccessToken)
        {
            _helper = new GitHubHelper(baseUrl, repositoryPath, personalAccessToken);
        }

        public async Task CloneAsync(ITemplatesProvider sourceProvider, Action<string> log)
        {
            throw new NotSupportedException();
        }

        public async Task<IEnumerable<ITemplateItem>> GetAsync(string path, Action<string> log)
        {
            var rateLimit = await _helper.GetRateLimitAsync();

            if (rateLimit.rate.remaining < 0 || rateLimit.resources.core.remaining < 0)
            {
                throw new InvalidOperationException("Request limit exceeded!");
            }

            if (String.IsNullOrWhiteSpace(path) || path == "/") path = "";

            ContentResponse[] response = await _helper.GetContentsAsync(path);

            return response.Select(r =>
            {
                if (r.type == "dir")
                    return (ITemplateItem)new TemplateDirectory(r);

                return new TemplateFile(_helper, r);
            });
        }

        public async Task<ITemplateAuthor> GetAuthorAsync(string path)
        {
            // Get the commits to retrieve the author
            var commits = await _helper.GetCommitsAsync(path);

            // Get the first commit
            var commit = commits.OrderBy(c => c.commit.author.date).FirstOrDefault();
            if (commit == null) return null;

            User author = await _helper.GetAuthorAsync(commit.author.login);
            if (commit == null) return null;

            return new TemplateAuthor(author);
        }

        private class TemplateAuthor : ITemplateAuthor
        {
            private User _user;

            public TemplateAuthor(User user)
            {
                this._user = user;
            }

            public string Name => _user.name;

            public string Email => _user.email;

            public string Link => _user.html_url;
        }

        private class TemplateItem : ITemplateItem
        {
            protected ContentResponse Response { get; }

            public TemplateItem(ContentResponse response)
            {
                Response = response;
            }

            public virtual string Path => Response.path;

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

        private class TemplateFile : TemplateItem, IMarkdownFile
        {
            private readonly GitHubHelper _helper;

            public TemplateFile(GitHubHelper helper, ContentResponse response) : base(response)
            {
                _helper = helper;
            }

            public Uri DownloadUri => new Uri(Response.download_url);

            public async Task<Stream> DownloadAsync()
            {               
                var client = _helper.GetNewHttpClient();
                HttpResponseMessage response = await client.GetAsync(DownloadUri, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStreamAsync(); ;
            }

            public async Task<string> GetHtmlAsync()
            {
                using (var reader = new StreamReader(await this.DownloadAsync()))
                {
                    string md = await reader.ReadToEndAsync();

                    return await _helper.GetHtmlFromMarkdownAsync(md);
                }
            }
        }

        private class TemplateDirectory : TemplateItem, ITemplateFolder
        {
            public TemplateDirectory(ContentResponse response) : base(response)
            {
            }

        }
    }
}
