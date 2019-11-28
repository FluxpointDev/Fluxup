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
            var json = responseMessage != null ? await responseMessage.Content.ReadAsStringAsync() : "";

            //Check to see if we got anything we can use
            if (string.IsNullOrEmpty(json))
            {
                IsCheckingForUpdate = false;
                return Logger.ErrorAndReturnDefault<GithubUpdateInfo>
                    ("\r\nWe are given no response from Github, can't continue..." +
                    responseMessage.ErrorResponseMessage());
            }
            else if (!(responseMessage?.IsSuccessStatusCode).GetValueOrDefault())
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
            var release = releases.Assets.Where(x => x.Name.StartsWith("RELEASES")).ToArray();
            releases.Assets = releases.Assets.Except(release).ToList();
            if (!release.Any())
            {
                IsCheckingForUpdate = false;
                return Logger.ErrorAndReturnDefault<GithubUpdateInfo>
                    ("They is no RELEASES file, assumed to have no updates");
            }
            else if (release.LongLength > 1)
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
                    if (!entry.CheckHash(File.Open($"../FluxupTemp/{entry.Filename}", FileMode.Open, FileAccess.Read), out _))
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
                if (!entry.CheckHash(localFile, out var SHA1Computed))
                {
                    localFile.Dispose();
                    continue;
                }
                
                downloadFailed?.Invoke(new SHA1MatchFailed(entry, SHA1Computed));
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

            //Get if the updates entries have the applications version,
            //if so we know that we can use delta packages :D
            var hasApplicationVersion = updateEntry.SkipWhile
                (x => x.Version != AppInfo.AppVersion.SystemToSemantic()).Any();
            
            //Sort the entries out, making it ordered in a way that we can use
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
            
            //Get the first update and get where the new update will be and make dir
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

                using var nugetFileLocation = File.Open(Path.Combine("..", "FluxupTemp", entry.Filename), FileMode.Open);
                string computedHash = null;
                if (!entry.CheckHash(nugetFileLocation, out computedHash))
                {
                    installFailed?.Invoke(new SHA1MatchFailed(entry, computedHash));
                    return;
                }
                using var nugetReader = new PackageArchiveReader(nugetFileLocation);

                //Gets the folder that will have content we need
                var folders = new List<NuGetFramework>();
                foreach (var file in await nugetReader.GetFilesAsync("lib", default))
                {
                    //TODO: Error out if we got more then two folder for use and if it doesn't have our .NET Version....
                    if (file.EndsWith(Path.DirectorySeparatorChar.ToString()) && file.Count(x => x == Path.DirectorySeparatorChar) == 2)
                    {
                        folders.Add(NuGetFramework.ParseFolder(file.Replace("lib/", "")
                            .Replace("/", "")));
                    }
                }

                string folderWithContent =
                    (folders.Count == 1 ? folders[0] :
                        folders.Contains(applicationFramework) ? applicationFramework : default)?.GetShortFolderName();

                if (string.IsNullOrEmpty(folderWithContent))
                {
                    installFailed?.Invoke(new FolderLocationUnavailable());
                    return;
                }

                string updatedFilesLocation;
                string[] currentVersionFiles = null;
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
                    currentVersionFiles = Directory.GetFiles(AppInfo.AppPath, "*", SearchOption.AllDirectories);
                    foreach (var file in currentVersionFiles)
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

                DeltaCompressionDotNet.IDeltaCompression MsDelta = null;
                updatedFilesLocation = Path.Combine("lib", folderWithContent, "");
                //var filesToCheck = new List<string>(currentVersionFiles);
                
                foreach (var deltaFile in nugetReader.GetItems(updatedFilesLocation).First(
                    x => x.TargetFramework.GetShortFolderName() == folderWithContent)?.Items ?? new List<string>())
                {
                    if (string.IsNullOrEmpty(deltaFile))
                    {
                        continue;
                    }
                    
                    //Where the updated file will be
                    var newFileLocation = Path.Combine(newVersionLocation, deltaFile.Replace(updatedFilesLocation + Path.DirectorySeparatorChar, ""));
                    if (deltaFile.EndsWith("/"))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(newFileLocation));
                        continue;
                    }

                    newFileLocation = 
                        newFileLocation.Remove(newFileLocation.IndexOf(Path.GetExtension(newFileLocation)));
                    //Where the file will be while being edited
                    var tmpFileLocation = Path.Combine("../FluxupTemp", deltaFile.Replace(updatedFilesLocation + Path.DirectorySeparatorChar, ""));

                    using var deltaFileStream = nugetReader.GetStream(deltaFile);
                    if (Path.GetExtension(deltaFile) == ".shasum")
                    {
                        if (File.Open(newFileLocation, FileMode.Open)
                            .CheckHash(await new StreamReader(deltaFileStream).ReadToEndAsync(),
                                out computedHash))
                        {
                            //filesToCheck.Remove(newFileLocation);
                            continue;
                        }
                        
                        installFailed?.Invoke(new SHA1MatchFailed(entry, computedHash));
                        return;
                    }

                    if (deltaFileStream.CanRead && deltaFileStream.ReadByte() == -1)
                    {
                        Logger.Information($"{deltaFile} has no content or is unreadable, skipping...");
                        continue;
                    }

                    var tmpStream = Stream.Null;
                    switch (Path.GetExtension(deltaFile))
                    {
                        //Error out if we aren't on windows as this format can only be used on windows
                        //(Why does it have to be the common format as well ;-;)
                        case ".diff" when !Core.OS.OperatingSystem.OnWindows:
                        {
                            installFailed?.Invoke
                            (new Exception("Can't apply this update due to being in the diff format " +
                                           "(Can only be applied in Windows due to it using a Windows only api)"));
                        }
                        return;
                        case ".diff":
                        {
                            //Copy delta file to the tmp file
                            tmpStream = File.OpenWrite(tmpFileLocation);
                            await deltaFileStream.CopyToAsync(tmpStream);
                            await tmpStream.FlushAsync();
                            tmpStream.Dispose();
                            tmpStream.Close();

                            MsDelta ??= new DeltaCompressionDotNet.MsDelta.MsDeltaCompression();

                            //Apply update and store in tmp folder, rename the old file, move new file and then delete the old file and the delta file
                            MsDelta.ApplyDelta(tmpFileLocation,newFileLocation, tmpFileLocation + ".new");
                            File.Move(newFileLocation, newFileLocation + ".old");
                            File.Move(tmpFileLocation + ".new", newFileLocation);
                            File.Delete(newFileLocation + ".old");
                            File.Delete(tmpFileLocation);
                        }
                        break;
                        case ".bsdiff":
                        {
                            tmpStream = new MemoryStream();
                            var memStream = new MemoryStream();
                            await deltaFileStream.CopyToAsync(memStream);
                            memStream.Seek(0, SeekOrigin.Begin);
                            deltaFileStream.Dispose();
                            
                            //Apply update, rename old file, write new file and delete old file
                            BsdiffPatchUtility.Apply(File.Open(newFileLocation, FileMode.Open),() => memStream, tmpStream);
                            File.Move(newFileLocation, newFileLocation + ".old");
                            using var newFileStream = File.Create(newFileLocation);
                            await tmpStream.CopyToAsync(newFileStream);
                            await newFileStream.FlushAsync();
                            File.Delete(newFileLocation + ".old");
                        }
                        break;
                        default:
                        {
                            //rename current file to .old
                            File.Move(newFileLocation, newFileLocation + ".old");

                            //Put the new contents in place
                            var newFileStream = File.Create(newFileLocation);
                            await deltaFileStream.CopyToAsync(newFileStream);
                            await newFileStream.FlushAsync();
                            
                            //Delete the old file
                            File.Delete(newFileLocation + ".old");
                            break;
                        }
                    }
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