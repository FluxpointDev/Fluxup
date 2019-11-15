using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using NuGet.Packaging;
using Newtonsoft.Json;
using Fluxup.Core;
using Fluxup.Core.Logging;
using Fluxup.Core.Networking;
using Fluxup.Updater.Exceptions;
using NuGet.Frameworks;

// ReSharper disable InconsistentNaming
namespace Fluxup.Updater.Github
{
    //TODO: Make it not use a load of ram when looking for files that show if it's a delta package
    /// <summary>
    /// Manages updates with Github as the update source 
    /// </summary>
    public class GithubUpdateFetcher : IUpdateFetcher<GithubUpdateInfo, GithubUpdateEntry>
    {
        internal const string GithubApiRoot = "https://api.github.com";

        private GithubUpdateFetcher @this;
        private readonly Logger Logger = new Logger(nameof(GithubUpdateFetcher));
        private GithubUpdateInfo LatestGithubUpdateInfo = new GithubUpdateInfo(null, true);

        /// <summary>
        /// Makes <see cref="GithubUpdateFetcher"/>
        /// </summary>
        /// <param name="applicationName">The applications name</param>
        /// <param name="ownerUsername">The owners username</param>
        /// <param name="repoName">The name of the repo the updates are sourced from</param>
        /// <param name="updateChannel">What channel you want to look at</param>
        public GithubUpdateFetcher(string applicationName, string ownerUsername, string repoName,
            string updateChannel = default)
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
        public bool IsInstalledApp { get; } = AppInfo.GetInstalledStatus();

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
                return Logger.ErrorAndReturnDefault<GithubUpdateInfo>
                    ("This isn't a installed application, you need to install this application.");
            }
#endif
            IsCheckingForUpdate = true;
            
            //Get json from release api
            using var httpClient = HttpClientHelper.CreateHttpClient(ApplicationName);
            using var responseMessage = await httpClient.GetAsyncLogged
                (GithubApiRoot + $"/repos/{OwnerUsername}/{RepoName}/releases/latest");
            var json = await responseMessage.Content.ReadAsStringAsync();

            //Check to see if we got anything we can use
            if (string.IsNullOrEmpty(json))
            {
                IsCheckingForUpdate = false;
                return Logger.ErrorAndReturnDefault<GithubUpdateInfo>
                    ("\r\nWe are given no response from Github, can't continue..." +
                    responseMessage.ErrorResponseMessage());
            }
            else if (!responseMessage.IsSuccessStatusCode)
            {
                var error = JsonConvert.DeserializeObject<GithubError>(json);
                IsCheckingForUpdate = false;
                return Logger.ErrorAndReturnDefault<GithubUpdateInfo>
                    ("\r\nResponse reports as unsuccessful. It's likely that you haven't uploaded your first update yet or typed your Github username/repo incorrectly." +
                    responseMessage.ErrorResponseMessage() +
                    $"\r\n  Message: {error.Message}" +
                    $"\r\n  Documentation Url: {error.DocumentationUrl}");
            }

            //Look for a RELEASES file from the release api 
            var releases = JsonConvert.DeserializeObject<GithubRelease>(json);
            var release = releases.Assets.Where(x => x.Name.StartsWith("RELEASES"));
            releases.Assets = releases.Assets.Except(release).ToList();
            if (!release.Any())
            {
                IsCheckingForUpdate = false;
                return Logger.ErrorAndReturnDefault<GithubUpdateInfo>
                    ("They is no RELEASES file, assumed to have no updates");
            }
            else if (release.Count() > 1)
            {
                //TODO: This is where we would use UpdateChannel, make it look at the filename to know what
                // file to use
                Logger.Warning("They is more then one RELEASES file, going to use the first RELEASES file");
            }

            //Grab RELEASES file and check if it's got any text
            //TODO: Check that RELEASES file is valid....
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

            //Get version and check if it is an delta package
            foreach (var asset in releases.Assets)
            {
                //Check that it's a file we can use
                if(!asset.Name.EndsWith(".nupkg") || !githubUpdateEntries.ContainsKey(asset.Name)) continue;
                githubUpdateEntries[asset.Name].DownloadUri = asset.BrowserDownloadUrl;

                //Add version and if it's a delta package from the filename if we can (and if it's a nuget file)
                if (githubUpdateEntries[asset.Name].AddVersionAndDeltaFromFileName(asset.Name)) continue;

                //Get nupkg file stream
                var fileStream = await httpClient.GetStreamAsyncLogged(asset.BrowserDownloadUrl);
                
                //get .nuspec file from nupkg
                using var packageReader = new PackageArchiveReader(fileStream);
                var nuspecReader = await packageReader.GetNuspecReaderAsync(default);
                githubUpdateEntries[asset.Name].Version = nuspecReader.GetMetadataValue("version").ParseVersion();
                
                //Check contents to see if it's a delta package
                //TODO: Find a way to await foreach this, would make this a *lot* faster and help with RAM usage
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
            IsCheckingForUpdate = false;
            return LatestGithubUpdateInfo = new GithubUpdateInfo(githubUpdateEntries.Values, useDeltaPatching);
        }
        
        /// <inheritdoc cref="Fluxup.Core.IUpdateFetcher{TUpdateInfo,TUpdateEntry}.DownloadUpdates(TUpdateEntry[], System.Action{System.Double}, System.Action{System.Exception})"/>
        public async Task DownloadUpdates(GithubUpdateEntry[] updateEntry, Action<double> totalProgress = default,
            Action<double, string> fileProgress = default, Action<Exception> downloadFailed = default)
        {
            //TODO: Check if updateEntry has got anything
#if !DEBUG
            //TODO: Check if we are already downloading the same updates....
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
                    //TODO: uncomment the if after, needed as osu!lazer hash's are not what they should be. :thonk:
                    //if (!entry.CheckHash(File.Open($"../FluxupTemp/{entry.Filename}", FileMode.Open, FileAccess.Read)))
                    //{
                    //    File.Delete($"../FluxupTemp/{entry.Filename}");
                    //}
                    //else
                    {
                        continue;
                    }
                }
                
                using var file = await httpClient.GetStreamAsyncLogged(entry.DownloadUri);
                using var localFile = new TrackableStream($"../FluxupTemp/{entry.Filename}", FileMode.Create);
                
                //TODO: Change this 
                //Gets how much bytes the file is (because for some reason they don't put in this length ;-;)
                var _contentBytesRemaining = file.GetType()
                    .GetMember("_contentBytesRemaining", BindingFlags.Instance | BindingFlags.NonPublic);
                var fileLength = double.Parse(((FieldInfo) _contentBytesRemaining[0]).GetValue(file).ToString());
                
                //Tell the user when the download has progressed
                localFile.LengthChanged += (_, amountDownloaded) =>
                {
                    totalProgress?.Invoke(((i + amountDownloaded / fileLength) / updateEntry.Length) * 100);
                    fileProgress?.Invoke((amountDownloaded / fileLength) * 100, entry.Filename);
                };
                await file.CopyToAsync(localFile);
                file.Dispose();

                // Check hash, if it's not the one we have throw error to the user,
                // delete the file and break as we can't continue due to this.
                //TODO: uncomment the if after, needed as osu!lazer hash's are not what they should be. :thonk:
                //if (entry.CheckHash(localFile))
                {
                    localFile.Dispose();
                    continue;
                }
                
                downloadFailed?.Invoke(new SHA1MatchFailed(entry.Filename, entry.SHA1, entry.SHA1Computed));
                localFile.Dispose();
                File.Delete($"../FluxupTemp/{entry.Filename}");
                IsDownloadingUpdates = false;
                break;
            }
            IsDownloadingUpdates = false;
        }

        /// <inheritdoc cref="Fluxup.Core.IUpdateFetcher{TUpdateInfo,TUpdateEntry}.InstallUpdates(TUpdateEntry[], System.Action{System.Double}, System.Action{System.Exception})"/>
        public async Task InstallUpdates(GithubUpdateEntry[] updateEntry, Action<double> progress = default, Action<Exception> installFailed = default)
        {
            //TODO: Check if updateEntry has got anything
#if !DEBUG
            //TODO: Check if we are already installing the same updates....
            if (!IsInstalledApp)
            {
                installFailed?.Invoke(new Exception("This isn't a installed application, you need to install this application."));
                return;
            }
#endif
            IsInstallingUpdates = true;

            //Get if the updates entries have the applications version, if so we know that we can use delta packages :D
            var hasApplicationVersion = updateEntry.SkipWhile
                (x => x.Version != AppInfo.AppVersion.SystemToSemantic()).Any();
            
            //Sort the entries out, making it ordered in a way for us to use
            updateEntry = updateEntry.OrderBy(x => x.IsDelta).ThenBy(x => x.Version).ToArray();
            var updateEntryTmp = updateEntry.ToList();
            for (var i = 0; i < updateEntryTmp.Count; i++)
            {
                //If the next two versions aren't the same then skip it (or if we are on the last entry)
                if (updateEntryTmp.Count == i + 1 || updateEntryTmp[i].Version != updateEntryTmp[i + 1].Version)
                {
                    continue;
                }
                
                //We want the full package if we don't have our current version with the packages as we don't
                //know what else would have changed ¯\_(ツ)_/¯
                if (!hasApplicationVersion)
                {
                    //Remove anything before the full version, they will be useless
                    if (updateEntryTmp[i].IsDelta && !updateEntryTmp[i + 1].IsDelta)
                    {
                        updateEntryTmp.RemoveRange(0, i + 1);
                    }
                    else if (!updateEntryTmp[i].IsDelta && updateEntryTmp[i + 1].IsDelta)
                    {
                        updateEntryTmp.RemoveRange(0, i);
                        updateEntryTmp.RemoveAt(1);
                    }
                }
                else
                {
                    //Remove the full packages, we don't want to use full packages as we already
                    //got the files needed
                    if (updateEntryTmp[i].IsDelta && !updateEntryTmp[i + 1].IsDelta)
                    {
                        updateEntryTmp.RemoveAt(i + 1);
                    }
                    else if (!updateEntryTmp[i].IsDelta && updateEntryTmp[i + 1].IsDelta)
                    {
                        updateEntryTmp.RemoveAt(i);
                    }
                }
            }
            //Check that we got *something* left and use it if we do
            updateEntry = updateEntryTmp.Any() ?
                updateEntryTmp.ToArray() : updateEntry;
            
            //Get the first update, get where the new update will be and make dir
            var firstUpdate = updateEntry[0];
            var newVersionLocation = Path.Combine("..", updateEntry[updateEntry.Length - 1].Version.ToString());
            Directory.CreateDirectory(newVersionLocation);

            //Get application framework and see if the package has 
            var applicationFramework = NuGetFramework.ParseFrameworkName(
                Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName,
                new DefaultFrameworkNameProvider());

            // TODO: Make sure the package file exists, if not then download it
            foreach (var entry in updateEntry)
            {
                if (!entry.IsDelta && firstUpdate != entry)
                {
                    installFailed?.Invoke(
                        new Exception($"Update {entry.Filename} shouldn't be a full package due to it not " +
                                                       "being the first update being applied"));
                }

                using var nugetFileLocation = File.Open(Path.Combine("..", "FluxupTemp", entry.Filename), FileMode.Open); //File.Open("/home/aaron/Downloads/package.nupkg", FileMode.Open); 
                using var nugetReader = new PackageArchiveReader(nugetFileLocation);

                //Gets the folder that will have content we need
                async Task<string> GetFolderWithContentNuGet()
                {
                    var folders = new List<NuGetFramework>();
                    foreach (var file in await nugetReader.GetFilesAsync("lib",default))
                    {
                        //TODO: Error out if we got more then two folder for use and if it doesn't have our .NET Version....
                        if (file.EndsWith(Path.DirectorySeparatorChar.ToString()) && file != "lib/" && file.Count(x => x == Path.DirectorySeparatorChar) == 2)
                        {
                            folders.Add(NuGetFramework.ParseFolder(file.Replace("lib/", "").Replace("/", "")));
                        }
                    }

                    return (folders.Count == 1 ? folders[0] :
                            folders.Contains(applicationFramework) ? applicationFramework : default)?.GetShortFolderName();
                }

                string folderWithContent;
                if (string.IsNullOrEmpty(folderWithContent = await GetFolderWithContentNuGet()))
                {
                    installFailed?.Invoke(new FolderLocationUnavailable());
                    return;
                }

                string updatedFilesLocation;
                if (firstUpdate == entry)
                {
                    if (!entry.IsDelta)
                    {
                        updatedFilesLocation = Path.Combine("lib", folderWithContent, "");
                        foreach (var file in nugetReader.GetItems(updatedFilesLocation)
                            .First(x => x.TargetFramework.GetShortFolderName() == folderWithContent)?.Items ?? default)
                        {
                            if (string.IsNullOrEmpty(file))
                            {
                                continue;
                            }
                            
                            var newFileLocation = Path.Combine(newVersionLocation, 
                                file.Replace(updatedFilesLocation + Path.DirectorySeparatorChar, ""));
                            if (file.EndsWith("/"))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(newFileLocation));
                                continue;
                            }
                                
                            using var fileStream = nugetReader.GetStream(file);
                            using var localFileStream = File.Create(newFileLocation);
                            await fileStream.CopyToAsync(localFileStream);
                            await localFileStream.FlushAsync();
                        }
                        continue;
                    }
                    
                    //If it gets here then the first update is a delta package, need the original files first to
                    //change, going to copy current files to where the updated version will be
                    foreach (var file in Directory.GetFiles(AppInfo.AppPath, "*",SearchOption.AllDirectories))
                    {
                        var dirName = Path.GetDirectoryName(file);
                        var newFileLocation = Path.Combine(dirName.Replace(AppInfo.AppPath, ""), Path.GetFileName(file));
                        if (newFileLocation.StartsWith(Path.DirectorySeparatorChar.ToString()))
                        {
                            newFileLocation = newFileLocation.Remove(0, 1);
                        }
                        newFileLocation = Path.Combine(newVersionLocation, newFileLocation);
                        
                        if (!Directory.Exists(Path.GetDirectoryName(newFileLocation)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(newFileLocation));
                        }
                        File.Copy(file, newFileLocation, true);
                    }
                }
                
                //Will be a delta package if it got here, work from that
                //TODO: Delete the file off the system it is not in the folder as it means that file is no longer needed
                //TODO: Check file after doing delta logic
                
                updatedFilesLocation = Path.Combine("lib", folderWithContent, "");
                foreach (var file in nugetReader.GetItems(updatedFilesLocation).First(
                    x => x.TargetFramework.GetShortFolderName() == folderWithContent)?.Items ?? new List<string>())
                {
                    if (string.IsNullOrEmpty(file))
                    {
                        continue;
                    }
                    
                    //Where the updated file will be
                    var newFileLocation = Path.Combine(newVersionLocation, file.Replace(updatedFilesLocation + Path.DirectorySeparatorChar, ""));
                    if (file.EndsWith("/"))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(newFileLocation));
                        continue;
                    }
                    //Where the file will be while being edited
                    var tmpFileLocation = Path.Combine("../FluxupTemp", file.Replace(updatedFilesLocation + Path.DirectorySeparatorChar, ""));

                    //TODO: Use this to check the file that would of just been processed
                    if (Path.GetExtension(file) == ".shasum")
                    {
                        continue;
                    }

                    using var deltaFileStream = nugetReader.GetStream(file);
                    if (deltaFileStream.CanRead && deltaFileStream.ReadByte() == -1)
                    {
                        Logger.Information($"{file} has no content, skipping...");
                        continue;
                    }
                    
                    using var tmpLocalFileStream = File.Create(tmpFileLocation);
                    if (Path.GetExtension(file) == ".diff")
                    {
                        //Error out if we aren't on windows as this format can only be used on windows
                        //(Why does it have to be the common format as well ;-;)
                        if (!Core.OS.OperatingSystem.OnWindows)
                        {
                            installFailed.Invoke(new Exception("Can't apply this update due to being in the diff format (Can only be used in Windows)"));
                            return;
                        }
                        
                        await deltaFileStream.CopyToAsync(tmpLocalFileStream);
                        await tmpLocalFileStream.FlushAsync();
                        tmpLocalFileStream.Dispose();
                        tmpLocalFileStream.Close();
                        new DeltaCompressionDotNet.MsDelta.MsDeltaCompression().ApplyDelta(tmpFileLocation, file.Remove(file.IndexOf(".diff")).Replace(updatedFilesLocation + Path.DirectorySeparatorChar, ""), newFileLocation);
                    }
                    else
                    {
                        var memStream = new MemoryStream();
                        await deltaFileStream.CopyToAsync(memStream);
                        memStream.Seek(0, SeekOrigin.Begin);

                        BinaryPatchUtility.Apply(File.Open(newFileLocation.Remove(newFileLocation.IndexOf(".diff")), FileMode.Open),() => memStream, tmpLocalFileStream);
                    }
                    //TODO: Finish this lol
                }
            }

            IsInstallingUpdates = false;
        }

        /// <inheritdoc cref="Fluxup.Core.IUpdateFetcher{TUpdateInfo,TUpdateEntry}.DownloadUpdates(Action{double}, Action{Exception})"/>
        public async Task DownloadUpdates(GithubUpdateEntry[] updateEntry, Action<double> progress = default, Action<Exception> downloadFailed = default)
        {
            await DownloadUpdates(updateEntry, progress, default, downloadFailed);
        }

        /// <inheritdoc cref="Fluxup.Core.IUpdateFetcher{TUpdateInfo,TUpdateEntry}.DownloadUpdates(Action{double}, Action{Exception})"/>
        public async Task DownloadUpdates(Action<double> progress = default, Action<Exception> downloadFailed = default)
        {
            //Check for updates if last time we check resulted in no updates
            //and check again for if that's the case
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
            //and check again for if that's the case
            if (!LatestGithubUpdateInfo.HasUpdate)
            {
                await CheckForUpdate(LatestGithubUpdateInfo.UseDelta);
            }
            
            await InstallUpdates(LatestGithubUpdateInfo.Updates, progress, installFailed);
        }
    }
}