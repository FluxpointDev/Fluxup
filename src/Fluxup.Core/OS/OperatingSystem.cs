using System;
using System.Runtime.InteropServices;
using Fluxup.Core.Exceptions;

// ReSharper disable InconsistentNaming
// ReSharper disable AssignmentInConditionalExpression
namespace Fluxup.Core.OS
{
    /// <summary>
    /// Gets what Operating System your user is running
    /// </summary>
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

        /// <summary>
        /// Platform user is running
        /// </summary>
        public static OSPlatform OSPlatform { get; }

        /// <summary>
        /// User is running Windows
        /// </summary>
        public static bool OnWindows { get; }

        /// <summary>
        /// User is running macOS
        /// </summary>
        public static bool OnMacOS { get; }

        /// <summary>
        /// User is running Linux
        /// </summary>
        public static bool OnLinux { get; }
        
        //TODO: Find out how to know when user is on android
        /// <summary>
        /// User is running Android
        /// </summary>
        public static bool OnAndroid { get; }

        /// <summary>
        /// Runs Action based on OS user is on
        /// </summary>
        /// <param name="onWindows">Action to run when on Windows</param>
        /// <param name="onMacOS">Action to run when on macOS</param>
        /// <param name="onLinux">Action to run when on Linux</param>
        /// <param name="onAndroid">Action to run when on Android</param>
        /// <exception cref="OSUnknownException">User is running a OS we don't know about</exception>
        public static void RunBasedOnPlatform(
            Action onWindows = default,
            Action onMacOS = default,
            Action onLinux = default,
            Action onAndroid = default)
        {
            switch (OSPlatform)
            {
                case OSPlatform.Windows:
                {
                    onWindows?.Invoke();
                }
                break;
                case OSPlatform.Linux:
                {
                    onLinux?.Invoke();
                }
                break;
                case OSPlatform.MacOS:
                {
                    onMacOS?.Invoke();
                }
                break;
                case OSPlatform.Android:
                {
                    onAndroid?.Invoke();
                }
                break;
                default:
                {
                    throw new OSUnknownException();
                }
            }
        }
    }
}