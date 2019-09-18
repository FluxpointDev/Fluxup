using System.Runtime.InteropServices;
using Fluxup.Updater.Exceptions;

namespace Fluxup.Updater
{
    public static class OperatingSystem
    {
        static OperatingSystem()
        {
            OnWindows = RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
            OnMacOS = RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX);
            OnLinux = RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
        }
        
        public static OSPlatform GetOSPlatform()
        {
            if (OnWindows)
            {
                return OSPlatform.Windows;
            }
            if (OnMacOS)
            {
                return OSPlatform.MacOS;
            }
            if (OnLinux)
            {
                return OSPlatform.Linux;
            }
            throw new OSUnknownException();
        }

        public static bool OnWindows { get; }
        public static bool OnMacOS { get; }
        public static bool OnLinux { get; }
    }
}