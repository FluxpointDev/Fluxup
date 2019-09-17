using System;
using System.Threading.Tasks;

namespace Fluxup.Updater
{
    public interface IUpdateFetcher
    {
        string ApplicationName { get; }
        string IsInstalledApp { get; }

        Task<IUpdateInfo> CheckForUpdate(bool useDeltaPatching = true);
        Task DownloadUpdates(IUpdateEntry[] updateEntry, Action<double> progress);
        Task DownloadUpdates(IUpdateInfo updateinfo, Action<double> progress);
        Task InstallUpdates(IUpdateInfo updateinfo, Action<double> progress);
    }
}
