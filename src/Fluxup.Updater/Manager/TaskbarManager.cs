using System;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo
namespace Fluxup.Updater.Manager
{
    /// <summary>
    /// Allows you to pin and unpin your application
    /// </summary>
    public static class TaskbarManager
    {
        /// <summary>
        /// If the application is pinned to the taskbar
        /// </summary>
        public static bool IsPinnedToTaskbar { get; }

        /// <summary>
        /// Pins your application to the taskbar
        /// </summary>
        /// <exception cref="NotImplementedException">This has not been implemented yet</exception>
        public static void PinToTaskbar()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Unpins your application to the taskbar
        /// </summary>
        /// <exception cref="NotImplementedException">This has not been implemented yet</exception>
        public static void UnpinToTaskbar()
        {
            throw new NotImplementedException();
        }
    }
}
