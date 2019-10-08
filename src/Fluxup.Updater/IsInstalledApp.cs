using System;
using System.IO;
using System.Reflection;
using Fluxup.Core.Exceptions;
using Fluxup.Core.Logging;
using Fluxup.Core.OS;
using OperatingSystem = Fluxup.Core.OS.OperatingSystem;

namespace Fluxup.Updater
{
    internal static class IsInstalledApp
    {
        private static readonly Logger Logger = new Logger("IsInstalledApp");
        private static readonly Assembly AppAssembly = Assembly.GetExecutingAssembly();
        private static readonly Version AppVersion = AppAssembly.GetName().Version;
        private static readonly string AppPath = Path.GetDirectoryName(AppAssembly.Location);
        
        /// <summary>
        /// Gets if the application is installed
        /// </summary>
        public static bool GetInstalledStatus()
        {
            return OperatingSystem.OSPlatform switch
            {
                OSPlatform.Windows => LinuxAndWindowsInstallStatus,
                OSPlatform.Linux => LinuxAndWindowsInstallStatus,
                OSPlatform.MacOS => false, //TODO: See how it sees applications packaged in a .app folder (or "macOS app")
                OSPlatform.Android => Logger.ErrorAndReturnDefault<bool>("This package doesn't work with Android..."),
                _ => throw new OSUnknownException()
            };
        }

        private static bool LinuxAndWindowsInstallStatus =>
            AppPath.EndsWith(AppVersion.ToString()) &&
            File.Exists(AppPath.Replace(AppVersion.ToString(),
            AppAssembly.GetName().Name + ExecutableFileType.GetExecutableFileType()));
    }
}