using System.Runtime.InteropServices;
using Fluxup.Updater.Exceptions;

namespace Fluxup.Updater
{
    public static class OperatingSystem
    {
        static OperatingSystem()
        {
            if (OnWindows = RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                OSPlatform = OSPlatform.Windows;
            }
            else if (OnMacOS = RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                OSPlatform = OSPlatform.MacOS;
            }
            else if (OnLinux = RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                OSPlatform = OSPlatform.Linux;
            }
            else
            {
                throw new OSUnknownException();
            }
        }

        public static OSPlatform OSPlatform { get; }
        public static bool OnWindows { get; }
        public static bool OnMacOS { get; }
        public static bool OnLinux { get; }
    }
}