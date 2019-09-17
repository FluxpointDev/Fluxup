using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fluxup.Updater.Github
{
    class GithubUpdateFetcher : IUpdateFetcher
    {
        public string ApplicationName => throw new NotImplementedException();

        public string IsInstalledApp => throw new NotImplementedException();

        public Task<IUpdateInfo> CheckForUpdate(bool useDeltaPatching = true)
        {
            throw new NotImplementedException();
        }

        public Task DownloadUpdates(IUpdateEntry[] updateEntry, Action<double> progress)
        {
            throw new NotImplementedException();
        }

        public Task DownloadUpdates(IUpdateInfo updateinfo, Action<double> progress)
        {
            throw new NotImplementedException();
        }

        public Task InstallUpdates(IUpdateInfo updateinfo, Action<double> progress)
        {
            throw new NotImplementedException();
        }
    }
}
