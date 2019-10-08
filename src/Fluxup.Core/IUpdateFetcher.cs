using System;
using System.Threading.Tasks;

namespace Fluxup.Core
{
    /// <summary>
    /// The update fetcher; contains everything needed for updating your application 
    /// </summary>
    /// <typeparam name="TUpdateInfo">The <see cref="IUpdateInfo{TUpdateEntry}"/> to be used</typeparam>
    /// <typeparam name="TUpdateEntry">The <see cref="IUpdateEntry"/> to use</typeparam>
    public interface IUpdateFetcher<TUpdateInfo, in TUpdateEntry>
    where TUpdateInfo : IUpdateInfo<TUpdateEntry>
    where TUpdateEntry : IUpdateEntry
    {
        /// <summary>
        /// The applications name
        /// </summary>
        string ApplicationName { get; }
        
        /// <summary>
        /// If this is a installed application
        /// </summary>
        bool IsInstalledApp { get; }
        
        /// <summary>
        /// If we are currently checking for updates
        /// </summary>
        bool IsCheckingForUpdate { get; }
        
        /// <summary>
        /// If we are downloading any updates
        /// </summary>
        bool IsDownloadingUpdates { get; }
        
        /// <summary>
        /// If we are installing any updates
        /// </summary>
        bool IsInstallingUpdates { get; }
        
        /// <summary>
        /// The "Channel" to check for any updates
        /// </summary>
        string UpdateChannel { get; set; }

        /// <summary>
        /// Checks for updates
        /// </summary>
        /// <param name="useDeltaPatching">If you want to use delta packages</param>
        /// <returns></returns>
        Task<TUpdateInfo> CheckForUpdate(bool useDeltaPatching = true);

        /// <summary>
        /// Downloads updates
        /// </summary>
        /// <param name="updateEntry">The updates to download</param>
        /// <param name="progress">Fires when download progress has been made</param>
        /// <param name="downloadFailed">Fires when downloading updates has failed</param>
        /// <returns>The updates being on the user's system</returns>
        Task DownloadUpdates(TUpdateEntry[] updateEntry, Action<double> progress = default, Action<Exception> downloadFailed = default);

        /// <summary>
        /// Downloads updates
        /// </summary>
        /// <param name="progress">Fires when download progress has been made</param>
        /// <param name="downloadFailed">Fires when downloading updates has failed</param>
        /// <returns>The updates being on the user's system</returns>
        Task DownloadUpdates(Action<double> progress = default, Action<Exception> downloadFailed = default);

        /// <summary>
        /// Installs updates
        /// </summary>
        /// <param name="updateEntry">The updates to Install</param>
        /// <param name="progress">Fires when progress has been made installing the updates</param>
        /// <param name="installFailed">Fires when the installation of updates has failed</param>
        /// <returns>A up-to-date Application</returns>
        Task InstallUpdates(TUpdateEntry[] updateEntry, Action<double> progress = default, Action<Exception> installFailed = default);

        /// <summary>
        /// Installs updates
        /// </summary>
        /// <param name="progress">Fires when progress has been made installing the updates</param>
        /// <param name="installFailed">Fires when the installation of updates has failed</param>
        /// <returns>A up-to-date Application</returns>
        Task InstallUpdates(Action<double> progress = default, Action<Exception> installFailed = default);
    }
}
