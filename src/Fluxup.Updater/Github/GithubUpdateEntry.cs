using System;
using SemVersion;
using System.Threading.Tasks;
using Fluxup.Core;
using Newtonsoft.Json;
using Fluxup.Core.Networking;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable IdentifierTypo
namespace Fluxup.Updater.Github
{
    /// <inheritdoc cref="Fluxup.Core.IUpdateEntry"/>
    public class GithubUpdateEntry : IUpdateEntry
    {
        internal GithubUpdateEntry(long releaseId, string sha1, string filename, long filesize, bool updateRequired, ref GithubUpdateFetcher githubUpdateFetcher)
        {
            ReleaseId = releaseId;
            GithubUpdateFetcher = githubUpdateFetcher;
            Filename = filename.Trim();
            Filesize = filesize;
            SHA1 = sha1.Trim();
            UpdateRequired = updateRequired;
        }

        /// <summary>
        /// Gets the hash computed from <see cref="GithubUpdateEntryEx.CheckHash(GithubUpdateEntry, System.IO.Stream)"/>
        /// </summary>
        internal string SHA1Computed { get; set; }

        /// <summary>
        /// Fetcher that made this <see cref="GithubUpdateEntry"/>
        /// </summary>
        private GithubUpdateFetcher GithubUpdateFetcher { get; }

        /// <summary>
        /// The uri to download the update
        /// </summary>
        internal Uri DownloadUri { get; set; }

        /// <summary>
        /// The id of this update on github
        /// </summary>
        public long ReleaseId { get; }

        /// <inheritdoc cref="Fluxup.Core.IUpdateEntry.Filename"/>
        public string Filename { get; }

        /// <inheritdoc cref="Fluxup.Core.IUpdateEntry.Filesize"/>
        public long Filesize { get; }

        /// <inheritdoc cref="Fluxup.Core.IUpdateEntry.IsDelta"/>
        public bool IsDelta { get; internal set; }

        /// <inheritdoc cref="Fluxup.Core.IUpdateEntry.SHA1"/>
        public string SHA1 { get; }

        /// <inheritdoc cref="Fluxup.Core.IUpdateEntry.Version"/>
        public SemanticVersion Version { get; internal set; }

        /// <inheritdoc cref="Fluxup.Core.IUpdateEntry.UpdateRequired"/>
        public bool UpdateRequired { get; }

        /// <inheritdoc cref="Fluxup.Core.IUpdateEntry.FetchReleaseNote()"/>
        public async Task<string> FetchReleaseNote()
        {
            using var httpClient = HttpClientHelper.CreateHttpClient(GithubUpdateFetcher.ApplicationName);
            using var jsonClient = await httpClient.GetAsyncLogged(GithubUpdateFetcher.GithubApiRoot + $"/repos/{GithubUpdateFetcher.OwnerUsername}/{GithubUpdateFetcher.RepoName}/releases/{ReleaseId}");
            var json = await jsonClient.Content.ReadAsStringAsync();
            var release = JsonConvert.DeserializeObject<GithubRelease>(json);
            return release.Body;
        }
    }
}
