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
        private static Logger Logger = new Logger("IsInstalledApp");
        private static Assembly appAssembly = Assembly.GetExecutingAssembly();
        private static Version appVersion = appAssembly.GetName().Version;
        private static string appPath = Path.GetDirectoryName(appAssembly.Location);
        
        public static bool GetInstalledStatus()
        {
            switch (OperatingSystem.OSPlatform)
            {
                case OSPlatform.Windows:
                case OSPlatform.Linux:
                    return appPath.EndsWith(appVersion.ToString()) &&
                           File.Exists(appPath.Replace(appVersion.ToString(),
                               appAssembly.GetName().Name + ExecutableFileType.GetExecutableFileType()));
                case OSPlatform.MacOS:
                    break; //TODO: See how it sees applications packaged in a .app folder (or "macOS app")
                case OSPlatform.Android:
                    return Logger.ErrorAndReturnDefault<bool>("This package doesn't work with Android...");
                default:
                    throw new OSUnknownException();
            }
            
            return false;
        }
    }
}