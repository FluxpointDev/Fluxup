using SemVersion;
using System.Threading.Tasks;
using Fluxup.Core;
using Newtonsoft.Json;
using Fluxup.Core.Networking;

namespace Fluxup.Updater.Github
{
    /// <summary>
    /// Application update
    /// </summary>
    public class GithubUpdateEntry : IUpdateEntry
    {
        internal GithubUpdateEntry(long releaseId, string sha1, string filename, long filesize, ref GithubUpdateFetcher githubUpdateFetcher)
        {
            ReleaseId = releaseId;
            GithubUpdateFetcher = githubUpdateFetcher;
            Filename = filename.Trim();
            Filesize = filesize;
            SHA1 = sha1.Trim();
        }

        /// <summary>
        /// Fetcher that made this
        /// </summary>
        private GithubUpdateFetcher GithubUpdateFetcher { get; }

        /// <summary>
        /// The id of this update on github
        /// </summary>
        public long ReleaseId { get; }

        /// <inheritdoc/>
        public string Filename { get; }

        /// <inheritdoc/>
        public long Filesize { get; }

        /// <inheritdoc/>
        public bool IsDelta { get; internal set; }

        /// <inheritdoc/>
        public string SHA1 { get; }

        /// <inheritdoc/>
        public SemanticVersion Version { get; internal set; }


        /// <inheritdoc/>
        public async Task<string> FetchReleaseNote()
        {
            using var httpClient = HttpClientHelper.CreateHttpClient(GithubUpdateFetcher.ApplicationName);
            var jsonClient = await httpClient.GetAsyncLogged(GithubUpdateFetcher.GithubApiRoot + $"/repos/{GithubUpdateFetcher.OwnerUsername}/{GithubUpdateFetcher.RepoName}/releases/{ReleaseId}");
            var json = await jsonClient.Content.ReadAsStringAsync();
            var release = JsonConvert.DeserializeObject<GithubRelease>(json);
            return release.Body;
        }
    }
}
