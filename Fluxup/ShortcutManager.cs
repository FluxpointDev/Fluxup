using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Fluxup.Updater
{
    /// <summary>
    /// Allows you to make and remove shortcut's
    /// </summary>
    public static class ShortcutManager
    {
        private const string WindowsFileType = ".Ink";
        private const string LinuxFileType = ".desktop";
        
        /// <summary>
        /// Makes a shortcut to the application.
        /// </summary>
        /// <param name="shortcutLocation">Where you want the shortcut to go.</param>
        /// <param name="iconLocation">Where the icon of the application is.</param>
        /// <param name="applicationCategories">Categories that the application fit into.</param>
        /// <param name="folderName">The folder that will contain the shortcut.</param>
        /// <param name="linuxUiLib">The GUI lib that the application using if being made *for* linux.</param>
        /// <exception cref="NotImplementedException">Windows and macOS is not added (yet)</exception>
        public static void CreateShortcut(ShortcutLocation shortcutLocation, string iconLocation = default, string folderName = default, ApplicationCategory[] applicationCategories = default, LinuxUILib linuxUiLib = LinuxUILib.None)
        {
            var shortcutContent = "";
            var assembly = Assembly.GetEntryAssembly();
            var assemblyName = assembly.GetName().Name;

            switch (OperatingSystem.OSPlatform)
            {
                case OSPlatform.Windows:
                    throw new NotImplementedException();
                case OSPlatform.Linux:
                {
                    var categoryContent = "";
                    var catCount = applicationCategories.Length;
                    var isTerminalApp = false;
                    var doneAudioVideoCheck = false;
                    for (var i = 0; i < catCount; i++)
                    {
                        categoryContent += applicationCategories[i] + ";";
                        switch (applicationCategories[i])
                        {
                            case ApplicationCategory.ConsoleOnly:
                                isTerminalApp = true;
                                break;
                            case ApplicationCategory.Audio:
                            case ApplicationCategory.Video:
                                if (!doneAudioVideoCheck && !applicationCategories.Contains(ApplicationCategory.AudioVideo))
                                {
                                    categoryContent += nameof(ApplicationCategory.AudioVideo) + ";";
                                    doneAudioVideoCheck = true;
                                }
                                break;
                        }
                    }

                    if (linuxUiLib != LinuxUILib.None)
                    {
                        categoryContent += linuxUiLib + ";";
                    }
                    shortcutContent =
                        "[Desktop Entry]\r\n" +
                        $"Name={assemblyName}\r\n" + //name of an app.
                        $"Exec={assembly.Location.Replace(".dll", "").Replace(" ", @"\ ")}\r\n" + //command used to launch an app.
                        $"Terminal={isTerminalApp.ToString().ToLower()}\r\n" + //whether an app requires to be run in a terminal.
                        $"Icon={iconLocation}\r\n" + //location of icon file.
                        "Type=Application\r\n" + //type
                        $"Categories=Application;{categoryContent}\r\n" + //categories in which this app should be listed.
                        $"Comment={assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()}"; //comment which appears as a tooltip.
                }
                break;
                case OSPlatform.MacOS:
                    Logging.TriggerLog("Can't manage shortcut's for macOS", LogLevel.Error);
                    return;
            }

            var shortcutFileLocation = GetShortcutFileLocation(shortcutLocation);
            if (!string.IsNullOrWhiteSpace(folderName))
            {
                shortcutFileLocation = Path.Combine(shortcutFileLocation, folderName);
                if (!Directory.Exists(shortcutFileLocation))
                {
                    Directory.CreateDirectory(shortcutFileLocation);
                }
            }
            
            File.WriteAllText(Path.Combine(shortcutFileLocation, assemblyName + GetShortcutFileType()), shortcutContent);
        }

        /// <summary>
        /// Removes shortcut
        /// </summary>
        /// <param name="shortcutLocation">Where you want the shortcut to go.</param>
        /// <param name="folderName">The folder that will contain the shortcut.</param>
        public static void RemoveShortcut(ShortcutLocation shortcutLocation, string folderName = default)
        {
            if (OperatingSystem.OnMacOS)
            {
                return;
            }

            var shortcutDirectory = GetShortcutFileLocation(shortcutLocation);
            var shortcutFileLocation = "";
            
            if (!string.IsNullOrWhiteSpace(folderName))
            {
                shortcutDirectory = Path.Combine(shortcutDirectory, folderName);
            }

            shortcutFileLocation = Path.Combine(shortcutDirectory, Assembly.GetEntryAssembly().GetName().Name + GetShortcutFileType());
            if (File.Exists(shortcutFileLocation))
            {
                File.Delete(shortcutFileLocation);
            }
            if (Directory.Exists(shortcutDirectory) && Directory.GetFiles(shortcutDirectory).Length == 0)
            {
                Directory.Delete(shortcutDirectory);
            }
        }

        /// <summary>
        /// Checks to see if the shortcut exists.
        /// </summary>
        /// <param name="shortcutLocation">Where you want the shortcut to go.</param>
        /// <param name="folderName">The folder that will contain the shortcut.</param>
        /// <returns>if the shortcut exists</returns>
        public static bool DoesShortcutExist(ShortcutLocation shortcutLocation, string folderName = default)
        {
            if (OperatingSystem.OnMacOS)
            {
                return false;
            }
            
            var shortcutFileLocation = GetShortcutFileLocation(shortcutLocation);
            if (!string.IsNullOrWhiteSpace(folderName))
            {
                shortcutFileLocation = Path.Combine(shortcutFileLocation, folderName);
            }

            return File.Exists(Path.Combine(shortcutFileLocation, Assembly.GetEntryAssembly().GetName().Name + GetShortcutFileType()));
        }

        /// <summary>
        /// Gets the initial location that the shortcut will be.
        /// </summary>
        /// <param name="shortcutLocation">Where you want the shortcut to go.</param>
        /// <returns></returns>
        private static string GetShortcutFileLocation(ShortcutLocation shortcutLocation)
        {
            return shortcutLocation == ShortcutLocation.StartMenu ? 
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) :
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        /// <summary>
        /// Gets the file type the shortcut will be using
        /// </summary>
        /// <returns>Shortcut file type</returns>
        private static string GetShortcutFileType()
        {
            switch (OperatingSystem.OSPlatform)
            {
                case OSPlatform.Windows:
                    return WindowsFileType;
                case OSPlatform.Linux:
                    return LinuxFileType;
                case OSPlatform.MacOS:
                    Logging.TriggerLog("Can't manage shortcut's for macOS", LogLevel.Error);
                    return "";
                default:
                    return "";
            }
        }
    }
}
