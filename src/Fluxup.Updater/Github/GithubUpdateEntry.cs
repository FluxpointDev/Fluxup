using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Fluxup.Updater.Github
{
    public class GithubUpdateEntry : IUpdateEntry
    {
        public GithubUpdateEntry(long releaseId, string sha1, string filename, long filesize, bool isDelta, ref GithubUpdateFetcher githubUpdateFetcher)
        {
            ReleaseId = releaseId.ToString();
            GithubUpdateFetcher = githubUpdateFetcher;
            Filename = filename;
            Filesize = filesize;
            IsDelta = isDelta;
            SHA1 = sha1;
        }

        /// <inheritdoc/>
        internal GithubUpdateFetcher GithubUpdateFetcher { get; }

        /// <inheritdoc/>
        public string ReleaseId { get; }

        /// <inheritdoc/>
        public string Filename { get; }

        /// <inheritdoc/>
        public long Filesize { get; }

        /// <inheritdoc/>
        public bool IsDelta { get; }

        /// <inheritdoc/>
        public string SHA1 { get; }

        /// <inheritdoc/>
        public Version Version => throw new NotImplementedException();

        /// <inheritdoc/>
        public async Task<string> FetchReleaseNote()
        {
            using (var httpClient = HttpClientHelper.CreateHttpClient(GithubUpdateFetcher.ApplicationName))
            {
                var jsonClient = await httpClient.GetAsync(GithubUpdateFetcher.GithubApiRoot + $"/repos/{GithubUpdateFetcher.OwnerUsername}/{GithubUpdateFetcher.RepoName}/releases/{ReleaseId}");
                var json = await jsonClient.Content.ReadAsStringAsync();
                var release = JsonConvert.DeserializeObject<GithubRelease>(json);
                return release.Body;
            }
        }
    }
}
