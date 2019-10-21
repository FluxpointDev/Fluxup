using Fluxup.Core;
using Fluxup.Core.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NuGet.Packaging;
using Fluxup.Core.Networking;
using Fluxup.Updater.Exceptions;

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
        private GithubUpdateInfo LatestGithubUpdateInfo = new GithubUpdateInfo(null, true);

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
        public bool IsDownloadingUpdates { get; private set; }

        /// <inheritdoc cref="Fluxup.Core.IUpdateFetcher{TUpdateInfo,TUpdateEntry}.IsInstallingUpdates"/>
        public bool IsInstallingUpdates { get; private set; }

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
#if !DEBUG
            //TODO: Check if we are already checking for a update....
            if (!IsInstalledApp)
            {
                return Logger.ErrorAndReturnDefault<GithubUpdateInfo>("This isn't a installed application, you need to install this application.");
            }
#endif
            IsCheckingForUpdate = true;
            
            //Get json from release api
            using var httpClient = HttpClientHelper.CreateHttpClient(ApplicationName);
            using var responseMessage = await httpClient.GetAsyncLogged(GithubApiRoot + $"/repos/{OwnerUsername}/{RepoName}/releases/latest");
            var json = await responseMessage.Content.ReadAsStringAsync();

            //Check to see if we got anything we can use
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

            //Look for a RELEASES file from the release api 
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

            //Grab RELEASES file and check if we can use it
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

            //Make RELEASES file contents into something we can use
            //TODO: Check that RELEASES file is valid....
            var releaseUpdates = releaseFile.Split('\r');
            var githubUpdateEntries = new Dictionary<string, GithubUpdateEntry>();
            foreach (var update in releaseUpdates)
            {
                if (string.IsNullOrEmpty(update.Trim()))
                {
                    continue;
                }

                var fileSplit = update.Split(' ');
                githubUpdateEntries.Add(fileSplit[1], new GithubUpdateEntry(releases.Id, fileSplit[0], fileSplit[1], long.Parse(fileSplit[2]), fileSplit.Length >= 4 && bool.Parse(fileSplit[3]), ref @this));
            }

            //Get version and if its a delta package
            foreach (var asset in releases.Assets)
            {
                //Check that it's a file we can use
                if(!asset.Name.EndsWith(".nupkg") ||
                   asset.Name == "RELEASES" || !githubUpdateEntries.ContainsKey(asset.Name)) continue;
                githubUpdateEntries[asset.Name].DownloadUri = asset.BrowserDownloadUrl;

                //Add version and if its a delta package from filename if we can (and if it's a nuget file)
                if (githubUpdateEntries[asset.Name].AddVersionAndDeltaFromFileName(asset.Name)) continue;

                //Get nupkg file stream
                var fileStream = await httpClient.GetStreamAsyncLogged(asset.BrowserDownloadUrl);
                
                //get .nuspec file from nupkg
                using var packageReader = new PackageArchiveReader(fileStream);
                var nuspecReader = await packageReader.GetNuspecReaderAsync(default);
                githubUpdateEntries[asset.Name].Version = nuspecReader.GetMetadataValue("version").ParseVersion();
                
                //Check contents to see if it's a delta package
                //TODO: Find a way to await foreach this, would make this a *lot* faster
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

            //Give it to people
            GC.Collect();
            IsCheckingForUpdate = false;
            return LatestGithubUpdateInfo = new GithubUpdateInfo(githubUpdateEntries.Values, useDeltaPatching);
        }

        /// <inheritdoc cref="Fluxup.Core.IUpdateFetcher{TUpdateInfo,TUpdateEntry}.DownloadUpdates(TUpdateEntry[], System.Action{System.Double}, System.Action{System.Exception})"/>
        public async Task DownloadUpdates(GithubUpdateEntry[] updateEntry, Action<double> progress = default, Action<Exception> downloadFailed = default)
        {
#if !DEBUG
            //TODO: Check if we are already downloading some updates....
            if (!IsInstalledApp)
            {
                Logger.Error("This isn't a installed application, you need to install this application.");
                return;
            }
#endif
            IsDownloadingUpdates = true;

            if (!Directory.Exists("../FluxupTemp"))
            {
                Directory.CreateDirectory("../FluxupTemp");
            }

            using var httpClient = HttpClientHelper.CreateHttpClient(ApplicationName);
            for (var i = 0; i < updateEntry.LongLength; i++)
            {
                var entry = updateEntry[i];
                // If the file exists then look if it's the file we need, if it is then use it else 
                // delete the file 
                if (File.Exists($"../FluxupTemp/{entry.Filename}"))
                {
                    if (!entry.CheckHash(File.Open($"../FluxupTemp/{entry.Filename}", FileMode.Open)))
                    {
                        File.Delete($"../FluxupTemp/{entry.Filename}");
                    }
                    else
                    {
                        continue;
                    }
                }
                
                using var file = await httpClient.GetStreamAsyncLogged(entry.DownloadUri);
                using var localFile = new TrackableStream($"../FluxupTemp/{entry.Filename}", FileMode.Create);

                //Gets how much bytes the file is (because for some reason they don't put in this length ;-;)
                var _contentBytesRemaining = file.GetType()
                    .GetMember("_contentBytesRemaining", BindingFlags.Instance | BindingFlags.NonPublic);
                var fileLength = double.Parse(((FieldInfo) _contentBytesRemaining[0]).GetValue(file).ToString());
                
                //Tell the user that it has progressed
                localFile.LengthChanged += (_, amountDownloaded) =>
                {
                    progress?.Invoke(((i + amountDownloaded / fileLength) / updateEntry.Length) * 100);
                };
                await file.CopyToAsync(localFile);
                file.Dispose();

                // Check hash, if it's not the one we have throw error to the user,
                // delete the file and break as we can't continue due to this.
                if (entry.CheckHash(localFile)) continue;
                downloadFailed?.Invoke(new SHA1MatchFailed(entry.Filename, entry.SHA1, entry.SHA1Computed));
                localFile.Dispose();
                File.Delete($"../FluxupTemp/{entry.Filename}");
                IsDownloadingUpdates = false;
                break;
            }
        }

        /// <inheritdoc cref="Fluxup.Core.IUpdateFetcher{TUpdateInfo,TUpdateEntry}.InstallUpdates(TUpdateEntry[], System.Action{System.Double}, System.Action{System.Exception})"/>
        public async Task InstallUpdates(GithubUpdateEntry[] updateEntry, Action<double> progress = default, Action<Exception> installFailed = default)
        {
#if !DEBUG
            //TODO: Check if we are already downloading some updates....
            if (!IsInstalledApp)
            {
                Logger.Error("This isn't a installed application, you need to install this application.");
                return;
            }
            IsInstallingUpdates = true;
#endif
            
            foreach (var entry in updateEntry)
            {
                if (entry.IsDelta)
                {
                    //TODO: Do IsDelta logic....
                }
                else
                {
                    //TODO: Do !IsDelta logic...
                }
            }

            IsInstallingUpdates = false;
        }

        /// <inheritdoc cref="Fluxup.Core.IUpdateFetcher{TUpdateInfo,TUpdateEntry}.DownloadUpdates(Action{double}, Action{Exception})"/>
        public async Task DownloadUpdates(Action<double> progress = default, Action<Exception> downloadFailed = default)
        {
            //Check for updates if last time we check resulted in no updates
            if (!LatestGithubUpdateInfo.HasUpdate)
            {
                await CheckForUpdate(LatestGithubUpdateInfo.UseDelta);
            }

            await DownloadUpdates(LatestGithubUpdateInfo.Updates, progress, downloadFailed);
        }
        
        /// <inheritdoc cref="Fluxup.Core.IUpdateFetcher{TUpdateInfo,TUpdateEntry}.InstallUpdates(Action{double}, Action{Exception})"/>
        public async Task InstallUpdates(Action<double> progress = default, Action<Exception> installFailed = default)
        {
            //Check for updates if last time we check resulted in no updates
            if (!LatestGithubUpdateInfo.HasUpdate)
            {
                await CheckForUpdate(LatestGithubUpdateInfo.UseDelta);
            }
            
            await InstallUpdates(LatestGithubUpdateInfo.Updates, progress, installFailed);
        }
    }
}
