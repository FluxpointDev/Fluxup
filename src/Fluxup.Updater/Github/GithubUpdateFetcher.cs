using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Fluxup.Updater.Github
{
    public class GithubUpdateFetcher : IUpdateFetcher<GithubUpdateInfo, GithubUpdateEntry>
    {
        private GithubUpdateFetcher @this;
        internal const string GithubApiRoot = "https://api.github.com";

        public GithubUpdateFetcher(string applicationName, string ownerUsername, string repoName, string updateChannel = default)
        {
            @this = this;
            ApplicationName = applicationName;
            OwnerUsername = ownerUsername;
            RepoName = repoName;
            UpdateChannel = updateChannel;
        }

        public string ApplicationName { get; }
        public string IsInstalledApp => throw new NotImplementedException();
        public bool IsCheckingForUpdate { get; }
        public bool IsDownloadingUpdates { get; }    
        public bool IsInstallingUpdates { get; }
        public string OwnerUsername { get; }
        public string RepoName { get; }
        public string UpdateChannel { get; }

        public async Task<GithubUpdateInfo> CheckForUpdate(bool useDeltaPatching = true)
        {
            using (var httpClient = HttpClientHelper.CreateHttpClient(ApplicationName))
            {
                var jsonClient = await httpClient.GetAsync(GithubApiRoot + $"/repos/{OwnerUsername}/{RepoName}/releases/latest");
                var json = await jsonClient.Content.ReadAsStringAsync();
                var release = JsonConvert.DeserializeObject<GithubRelease>(json);
                var githubUpdateEntrys = new List<GithubUpdateEntry>();

                var releaseFile = "";
                foreach (var asset in release.Assets)
                {
                    if (asset.Name == "RELEASES")
                    {
                        var releaseFileContent = await httpClient.GetAsync(asset.BrowserDownloadUrl);
                        releaseFile = await releaseFileContent.Content.ReadAsStringAsync();
                        var releaseUpdate = releaseFile.Split('\r');
                        foreach (var item in releaseUpdate)
                        {
                            if (string.IsNullOrEmpty(item.Trim()))
                            {
                                continue;
                            }

                            var fileSplit = item.Split(' ');
                            bool isDelta = false;
                            if (fileSplit.Length == 4 && fileSplit[3] == "1")
                            {
                                isDelta = true;
                            }
                            //System.IO.Compression.ZipFile.Open(re) //Get if delta
                            githubUpdateEntrys.Add(new GithubUpdateEntry(release.Id, fileSplit[0], fileSplit[1], long.Parse(fileSplit[2]), isDelta, ref @this));
                        }
                    }
                }
                return new GithubUpdateInfo(useDeltaPatching ? githubUpdateEntrys : githubUpdateEntrys.Where(x => x.IsDelta == false));
            }
        }

        public Task DownloadUpdates(GithubUpdateEntry[] updateEntry, Action<double> progress = default, Action<Exception> downloadFailed = default)
        {
            throw new NotImplementedException();
        }

        public Task DownloadUpdates(Action<double> progress = default, Action<Exception> downloadFailed = default)
        {
            throw new NotImplementedException();
        }

        public Task InstallUpdates(GithubUpdateEntry[] updateEntry, Action<double> progress = default, Action<Exception> installFailed = default)
        {
            throw new NotImplementedException();
        }

        public Task InstallUpdates(Action<double> progress = default, Action<Exception> installFailed = default)
        {
            throw new NotImplementedException();
        }
    }
}
