using System;
using System.Threading.Tasks;

namespace Fluxup.Core
{
    public interface IUpdateFetcher<TUpdateInfo, TUpdateEntry>
    where TUpdateInfo : IUpdateInfo<TUpdateEntry>
    where TUpdateEntry : IUpdateEntry
    {
        string ApplicationName { get; }
        string IsInstalledApp { get; }
        bool IsCheckingForUpdate { get; }
        bool IsDownloadingUpdates { get; }
        bool IsInstallingUpdates { get; }
        string UpdateChannel { get; }

        Task<TUpdateInfo> CheckForUpdate(bool useDeltaPatching = true);
        Task DownloadUpdates(TUpdateEntry[] updateEntry, Action<double> progress = default, Action<Exception> downloadFailed = default);
        Task DownloadUpdates(Action<double> progress = default, Action<Exception> downloadFailed = default);
        Task InstallUpdates(TUpdateEntry[] updateEntry, Action<double> progress = default, Action<Exception> installFailed = default);
        Task InstallUpdates(Action<double> progress = default, Action<Exception> installFailed = default);
    }
}
