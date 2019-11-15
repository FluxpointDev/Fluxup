using Fluxup.Core.Exceptions;
using Fluxup.Core.Logging;
using Fluxup.Core.OS;

namespace Fluxup.Updater
{
    internal static class ExecutableFileType
    {
        private static readonly Logger Logger = new Logger(nameof(ExecutableFileType));
        
        /// <summary>
        /// Gets the executable file type of the application
        /// </summary>
        public static string GetExecutableFileType()
        {
            return OperatingSystem.OSPlatform switch
            {
                OSPlatform.Windows => ".exe",
                OSPlatform.MacOS => ".app",
                OSPlatform.Linux => "",
                OSPlatform.Android => Logger.ErrorAndReturnDefault<string>("This package doesn't work with Android..."),
                _ => throw new OSUnknownException()
            };
        }
    }
}