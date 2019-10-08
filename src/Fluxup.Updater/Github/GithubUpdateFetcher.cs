using Fluxup.Core;
using Fluxup.Core.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Packaging;
using Fluxup.Core.Networking;

// ReSharper disable InconsistentNaming
namespace Fluxup.Updater.Github
{
    //TODO: Make it not use a load of ram when looking for files that show if it's a delta package
    /// <summary>
    /// Manage updates with using Github as your update source 
    /// </summary>
    public class GithubUpdateFetcher : IUpdateFetcher<GithubUpdateInfo, GithubUpdateEntry>
    {
        private GithubUpdateFetcher @this;
        internal const string GithubApiRoot = "https://api.github.com";
        private readonly Logger Logger = new Logger("GithubUpdateFetcher");

        /// <summary>
        /// Makes <see cref="GithubUpdateFetcher"/>
        /// </summary>
        /// <param name="applicationName">The applications name</param>
        /// <param name="ownerUsername">The owners username</param>
        /// <param name="repoName">The name of the repo the updates are sourced from</param>
        /// <param name="updateChannel">What channel you want to look at</param>
        public GithubUpdateFetcher(string applicationName, string ownerUsername, string repoName, string updateChannel = default)
        {
            @this = this;
            ApplicationName = applicationName;
            OwnerUsername = ownerUsername;
            RepoName = repoName;
            UpdateChannel = updateChannel;
        }

        /// <inheritdoc cref="Fluxup.Core.IUpdateFetcher{TUpdateInfo,TUpdateEntry}.ApplicationName"/>
        public string ApplicationName { get; }

        /// <inheritdoc cref="Fluxup.Core.IUpdateFetcher{TUpdateInfo,TUpdateEntry}.IsInstalledApp"/>
        public bool IsInstalledApp { get; } = Updater.IsInstalledApp.GetInstalledStatus();

        /// <inheritdoc cref="Fluxup.Core.IUpdateFetcher{TUpdateInfo,TUpdateEntry}.IsCheckingForUpdate"/>
        public bool IsCheckingForUpdate { get; private set; }

        /// <inheritdoc cref="Fluxup.Core.IUpdateFetcher{TUpdateInfo,TUpdateEntry}.IsDownloadingUpdates"/>
        public bool IsDownloadingUpdates { get; }

        /// <inheritdoc cref="Fluxup.Core.IUpdateFetcher{TUpdateInfo,TUpdateEntry}.IsInstallingUpdates"/>
        public bool IsInstallingUpdates { get; }

        //TODO: Use this when getting updates
        /// <inheritdoc cref="Fluxup.Core.IUpdateFetcher{TUpdateInfo,TUpdateEntry}.UpdateChannel"/>
        public string UpdateChannel { get; set; }

        /// <summary>
        /// The owners username
        /// </summary>
        public string OwnerUsername { get; }

        /// <summary>
        /// The repos name
        /// </summary>
        public string RepoName { get; }

        /// <inheritdoc cref="Fluxup.Core.IUpdateFetcher{TUpdateInfo,TUpdateEntry}.CheckForUpdate(bool)"/>
        public async Task<GithubUpdateInfo> CheckForUpdate(bool useDeltaPatching = true)
        {
            IsCheckingForUpdate = true;
            
            using var httpClient = HttpClientHelper.CreateHttpClient(ApplicationName);
            using var responseMessage = await httpClient.GetAsyncLogged(GithubApiRoot + $"/repos/{OwnerUsername}/{RepoName}/releases/latest");

            var json = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(json))
            {
                IsCheckingForUpdate = false;
                return Logger.ErrorAndReturnDefault<GithubUpdateInfo>
                    ("\r\nWe are given no response from github, can't continue..." +
                    responseMessage.ErrorResponseMessage());
            }
            else if (!responseMessage.IsSuccessStatusCode)
            {
                var error = JsonConvert.DeserializeObject<GithubError>(json);
                IsCheckingForUpdate = false;
                return Logger.ErrorAndReturnDefault<GithubUpdateInfo>
                    ($"\r\nResponse gave a unsuccessful status code, maybe you haven't released a update yet or typed your github username/repo incorrectly?" +
                    responseMessage.ErrorResponseMessage() +
                    $"\r\n  Message: {error.Message}" +
                    $"\r\n  Documentation Url: {error.DocumentationUrl}");
            }

            var releases = JsonConvert.DeserializeObject<GithubRelease>(json);
            var release = releases.Assets.Where(x => x.Name == "RELEASES");
            if (!release.Any())
            {
                IsCheckingForUpdate = false;
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
                IsCheckingForUpdate = false;
                return Logger.ErrorAndReturnDefault<GithubUpdateInfo>
                    ("\r\nSomething happened while getting the RELEASES file" +
                    responseMessage.ErrorResponseMessage());
            }
            else if (string.IsNullOrEmpty(releaseFile))
            {
                IsCheckingForUpdate = false;
                return Logger.ErrorAndReturnDefault<GithubUpdateInfo>
                    ("RELEASES file has no content");
            }

            var releaseUpdates = releaseFile.Split('\r');
            var githubUpdateEntries = new Dictionary<string, GithubUpdateEntry>();
            foreach (var update in releaseUpdates)
            {
                if (string.IsNullOrEmpty(update.Trim()))
                {
                    continue;
                }

                var fileSplit = update.Split(' ');
                githubUpdateEntries.Add(fileSplit[1], new GithubUpdateEntry(releases.Id, fileSplit[0], fileSplit[1], long.Parse(fileSplit[2]), ref @this));
            }

            foreach (var asset in releases.Assets)
            {
                if (!asset.Name.EndsWith(".nupkg") ||
                    asset.Name == "RELEASES" || !githubUpdateEntries.ContainsKey(asset.Name) ||
                    githubUpdateEntries[asset.Name].AddVersionAndDeltaFromFileName(asset.Name)) continue;
                
                var fileStream = await httpClient.GetStreamAsyncLogged(asset.BrowserDownloadUrl);
                
                using var packageReader = new PackageArchiveReader(fileStream);
                var nuspecReader = await packageReader.GetNuspecReaderAsync(default);
                githubUpdateEntries[asset.Name].Version = nuspecReader.GetMetadataValue("version").ParseVersion();

                foreach (var entry in await packageReader.GetFilesAsync(default))
                {
                    if (string.IsNullOrEmpty(entry) || entry.EndsWith("/"))
                    {
                        continue;
                    }
                    githubUpdateEntries[asset.Name].IsDelta = entry.EndsWith(".shasum") || entry.EndsWith(".diff");
                    break;
                }
            }

            GC.Collect();
            IsCheckingForUpdate = false;
            return new GithubUpdateInfo(githubUpdateEntries.Values);
        }

        /// <inheritdoc cref="Fluxup.Core.IUpdateFetcher{TUpdateInfo,TUpdateEntry}.DownloadUpdates(TUpdateEntry[], System.Action{System.Double}, System.Action{System.Exception})"/>
        public Task DownloadUpdates(GithubUpdateEntry[] updateEntry, Action<double> progress = default, Action<Exception> downloadFailed = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="Fluxup.Core.IUpdateFetcher{TUpdateInfo,TUpdateEntry}.DownloadUpdates(Action{double}, Action{Exception})"/>
        public Task DownloadUpdates(Action<double> progress = default, Action<Exception> downloadFailed = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="Fluxup.Core.IUpdateFetcher{TUpdateInfo,TUpdateEntry}.InstallUpdates(TUpdateEntry[], System.Action{System.Double}, System.Action{System.Exception})"/>
        public Task InstallUpdates(GithubUpdateEntry[] updateEntry, Action<double> progress = default, Action<Exception> installFailed = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="Fluxup.Core.IUpdateFetcher{TUpdateInfo,TUpdateEntry}.InstallUpdates(Action{double}, Action{Exception})"/>
        public Task InstallUpdates(Action<double> progress = default, Action<Exception> installFailed = default)
        {
            throw new NotImplementedException();
        }
    }
}
