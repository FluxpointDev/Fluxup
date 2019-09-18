using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fluxup.Updater.Github
{
    public class GithubUpdateFetcher : IUpdateFetcher<GithubUpdateInfo, GithubUpdateEntry>
    {
        public GithubUpdateFetcher(string applicationName, string ownerUsername, string repoName, string updateChannel = default)
        {
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

        public Task<GithubUpdateInfo> CheckForUpdate(bool useDeltaPatching = true)
        {
            throw new NotImplementedException();
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
