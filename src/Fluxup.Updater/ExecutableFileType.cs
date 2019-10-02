using System;
using Fluxup.Core.Exceptions;
using Fluxup.Core.Logging;
using Fluxup.Core.OS;

namespace Fluxup.Updater
{
    internal static class ExecutableFileType
    {
        private static Logger Logger = new Logger("ExecutableFileType");
        
        public static string GetExecutableFileType()
        {
            switch (Core.OS.OperatingSystem.OSPlatform)
            {
                case OSPlatform.Windows:
                    return ".exe";
                case OSPlatform.MacOS:
                    return ".app";
                case OSPlatform.Linux:
                    return "";
                case OSPlatform.Android:
                    return Logger.ErrorAndReturnDefault<string>("This package doesn't work with Android...");
                default:
                    throw new OSUnknownException();
            }
        }
    }
}