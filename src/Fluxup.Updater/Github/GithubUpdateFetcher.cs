using Fluxup.Core;
using Fluxup.Core.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Packaging;
using Fluxup.Core.Networking;

namespace Fluxup.Updater.Github
{
    //TODO: Make it not use a load of ram when looking for files that show if it's a delta package

    public class GithubUpdateFetcher : IUpdateFetcher<GithubUpdateInfo, GithubUpdateEntry>
    {
        private GithubUpdateFetcher @this;
        internal const string GithubApiRoot = "https://api.github.com";
        private readonly Logger Logger = new Logger("GithubUpdateFetcher");

        public GithubUpdateFetcher(string applicationName, string ownerUsername, string repoName, string updateChannel = default)
        {
            @this = this;
            ApplicationName = applicationName;
            OwnerUsername = ownerUsername;
            RepoName = repoName;
            UpdateChannel = updateChannel;
        }

        /// <inheritdoc/>
        public string ApplicationName { get; }

        /// <inheritdoc/>
        public string IsInstalledApp => throw new NotImplementedException();

        /// <inheritdoc/>
        public bool IsCheckingForUpdate { get; }

        /// <inheritdoc/>
        public bool IsDownloadingUpdates { get; }

        /// <inheritdoc/>
        public bool IsInstallingUpdates { get; }

        /// <inheritdoc/>
        public string UpdateChannel { get; }

        /// <summary>
        /// 
        /// </summary>
        public string OwnerUsername { get; }

        /// <summary>
        /// 
        /// </summary>
        public string RepoName { get; }

        /// <inheritdoc/>
        public async Task<GithubUpdateInfo> CheckForUpdate(bool useDeltaPatching = true)
        {
            using var httpClient = HttpClientHelper.CreateHttpClient(ApplicationName);
            using var responseMessage = await httpClient.GetAsyncLogged(GithubApiRoot + $"/repos/{OwnerUsername}/{RepoName}/releases/latest");

            var json = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(json))
            {
                return Logger.ErrorAndReturnDefault<GithubUpdateInfo>
                    ("\r\nWe are given no response from github, can't continue..." +
                    responseMessage.ErrorResponseMessage());
            }
            else if (!responseMessage.IsSuccessStatusCode)
            {
                var error = JsonConvert.DeserializeObject<GithubError>(json);
                return Logger.ErrorAndReturnDefault<GithubUpdateInfo>
                    ($"\r\nResponse gave a unsuccessful status code, maybe you haven't released a update yet or typed your github username/repo incorrectly?" +
                    responseMessage.ErrorResponseMessage() +
                    $"\r\n  Message: {error.Message}" +
                    $"\r\n  Documentation Url: {error.DocumentationUrl}");
            }

            var releases = JsonConvert.DeserializeObject<GithubRelease>(json);
            var release = releases.Assets.Where(x => x.Name == "RELEASES");
            if (release.Count() == 0)
            {
                return Logger.ErrorAndReturnDefault<GithubUpdateInfo>
                    ("They is no RELEASES file, assumed to have no updates");
            }
            else if (release.Count() > 1)
            {
                Logger.Warning("They is more then one RELEASES file, going to use the first RELEASES file");
            }

            using var releaseFileContent = await httpClient.GetAsyncLogged(release.First().BrowserDownloadUrl);
            var releaseFile = await releaseFileContent.Content.ReadAsStringAsync();
            if (!releaseFileContent.IsSuccessStatusCode)
            {
                return Logger.ErrorAndReturnDefault<GithubUpdateInfo>
                    ("\r\nSomething happened while getting the RELEASES file" +
                    responseMessage.ErrorResponseMessage());
            }
            else if (string.IsNullOrEmpty(releaseFile))
            {
                return Logger.ErrorAndReturnDefault<GithubUpdateInfo>
                    ("RELEASES file has no content");
            }

            var releaseUpdates = releaseFile.Split('\r');
            var githubUpdateEntrys = new Dictionary<string, GithubUpdateEntry>();
            foreach (var update in releaseUpdates)
            {
                if (string.IsNullOrEmpty(update.Trim()))
                {
                    continue;
                }

                var fileSplit = update.Split(' ');
                githubUpdateEntrys.Add(fileSplit[1], new GithubUpdateEntry(releases.Id, fileSplit[0], fileSplit[1], long.Parse(fileSplit[2]), ref @this));
            }

            foreach (var asset in releases.Assets)
            {
                if (asset.Name != "RELEASES" && githubUpdateEntrys.ContainsKey(asset.Name) &&
                    !githubUpdateEntrys[asset.Name].AddVersionAndDeltaFromFileName(asset.Name))
                {
                    var fileStream = await httpClient.GetStreamAsyncLogged(asset.BrowserDownloadUrl);

                    using var packageReader = new PackageArchiveReader(fileStream);
                    var nuspecReader = await packageReader.GetNuspecReaderAsync(default);
                    githubUpdateEntrys[asset.Name].Version = nuspecReader.GetMetadataValue("version").ParseVersion();
                    foreach (var entry in await packageReader.GetFilesAsync(default))
                    {
                        if (string.IsNullOrEmpty(entry) || entry.EndsWith("/"))
                        {
                            continue;
                        }
                        githubUpdateEntrys[asset.Name].IsDelta = entry.EndsWith(".shasum") || entry.EndsWith(".diff");
                        break;
                    }
                }
            }

            GC.Collect();
            return new GithubUpdateInfo(githubUpdateEntrys.Values);
        }

        /// <inheritdoc/>
        public Task DownloadUpdates(GithubUpdateEntry[] updateEntry, Action<double> progress = default, Action<Exception> downloadFailed = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task DownloadUpdates(Action<double> progress = default, Action<Exception> downloadFailed = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task InstallUpdates(GithubUpdateEntry[] updateEntry, Action<double> progress = default, Action<Exception> installFailed = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task InstallUpdates(Action<double> progress = default, Action<Exception> installFailed = default)
        {
            throw new NotImplementedException();
        }
    }
}
