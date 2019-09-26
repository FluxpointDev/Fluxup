using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Fluxup.Core.Logging;
using Fluxup.Core.OS;

namespace Fluxup.Updater.Manager
{
    //TODO: Add macOS support
    //TODO: Find DLLImport for Shortcut making on Windows and replace current code with that
    /// <summary>
    /// Allows you to make and remove shortcut's
    /// </summary>
    public static class ShortcutManager
    {
        private const string WindowsFileType = ".lnk";
        private const string LinuxFileType = ".desktop";
        private static Logger Logger = new Logger("Shortcut");

        /// <summary>
        /// Makes a shortcut to the application.
        /// </summary>
        /// <param name="shortcutLocation">Where you want the shortcut to go.</param>
        /// <param name="iconLocation">Where the icon of the application is.</param>
        /// <param name="applicationCategories">Categories that the application fit into.</param>
        /// <param name="folderName">The folder that will contain the shortcut.</param>
        /// <param name="linuxUiLib">The GUI lib that the application using if being made *for* linux.</param>
        /// <exception cref="NotImplementedException">macOS is not added (yet)</exception>
        public static void CreateShortcut(ShortcutLocation shortcutLocation, string iconLocation = default, string folderName = default, ApplicationCategory[] applicationCategories = default, LinuxUILib linuxUiLib = LinuxUILib.None)
        {
            // Get common content that's used everywhere
            var assembly = Assembly.GetEntryAssembly();
            var assemblyName = assembly.GetName().Name;
            var assemblyDescription = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
            var shortcutFileLocation = GetShortcutFileLocation(shortcutLocation);

            // Make folder if they want it
            if (!string.IsNullOrWhiteSpace(folderName))
            {
                Logger.Debug($"Shortcut needs to be put in '{folderName}'");
                shortcutFileLocation = Path.Combine(shortcutFileLocation, folderName);
                if (!Directory.Exists(shortcutFileLocation))
                {
                    Logger.Debug($"Folder '{folderName}' doesn't exist, making it");
                    Directory.CreateDirectory(shortcutFileLocation);
                }
            }

            Logger.Debug($"assemblyName: {assemblyName}");
            Logger.Debug($"assemblyDescription: {assemblyDescription ?? "N/A"}");
            Logger.Debug($"shortcutFileLocation: {shortcutFileLocation}");

            switch (Core.OS.OperatingSystem.OSPlatform)
            {
                case OSPlatform.Windows:
                    {
                        //VB script to make shortcut
                        var vbsContent =
                            $"set oShellLink = WScript.CreateObject(\"WScript.Shell\").CreateShortcut(\"{shortcutFileLocation}\" & \"\\{assemblyName}.lnk\")\r\n" + //Makes the shortcut
                            $"oShellLink.TargetPath = \"{assembly.Location.Replace(".dll", ".exe")}\"\r\n" + //exe to load
                            (!string.IsNullOrEmpty(iconLocation) ? $"oShellLink.IconLocation = \"{iconLocation}\"\r\n" : "") + //Icon to show (.ico format)
                            (!string.IsNullOrEmpty(assemblyDescription) ? $"oShellLink.Description = \"{assemblyDescription}\"\r\n" : "") + //Application's description
                            $"oShellLink.WorkingDirectory = \"{Path.GetDirectoryName(assembly.Location.Replace(".dll", ".exe"))}\"\r\n" +
                            "oShellLink.Save()";

                        //Write, run and delete
                        File.WriteAllText("shortcut.vbs", vbsContent);
                        Logger.Debug($"Made shortcut.vbs file, going to run it using wscript");
                        Process.Start(new ProcessStartInfo("wscript", "shortcut.vbs")
                        {
                            CreateNoWindow = true
                        }).WaitForExit();
                        Logger.Debug($"Shortcut should have been made, deleting shortcut.vbs");
                        File.Delete("shortcut.vbs");
                    }
                    break;
                case OSPlatform.Linux:
                    {
                        var categoryContent = "";
                        var catCount = applicationCategories.Length;
                        var isTerminalApp = false;
                        var doneAudioVideoCheck = false;
                        Logger.Debug($"Amount of categories application applys to: {catCount}");

                        for (var i = 0; i < catCount; i++)
                        {
                            categoryContent += applicationCategories[i] + ";";
                            switch (applicationCategories[i])
                            {
                                case ApplicationCategory.ConsoleOnly:
                                    isTerminalApp = true;
                                    Logger.Debug($"Application is a console app");
                                    break;
                                case ApplicationCategory.Audio:
                                case ApplicationCategory.Video:
                                    if (!doneAudioVideoCheck)
                                    {
                                        doneAudioVideoCheck = true;
                                        if (applicationCategories.Contains(ApplicationCategory.AudioVideo)) continue;
                                        Logger.Warning("You didn't add 'ApplicationCategory.AudioVideo' to the list of categories, this is needed when having only Video or Audio (We have add it for you but please add it)");
                                        categoryContent += nameof(ApplicationCategory.AudioVideo) + ";";
                                    }
                                    break;
                            }
                        }

                        if (linuxUiLib != LinuxUILib.None)
                        {
                            categoryContent += linuxUiLib + ";";
                            Logger.Debug($"Application is a linux application");
                        }
                        var shortcutContent =
                            "[Desktop Entry]\r\n" +
                            $"Name={assemblyName}\r\n" + // Name of an app.
                            $"Exec={assembly.Location.Replace(".dll", "").Replace(" ", @"\ ")}\r\n" + // Command used to launch an app.
                            $"Terminal={isTerminalApp.ToString().ToLower()}\r\n" + // Whether an app requires to be run in a terminal.
                            $"Icon={iconLocation}\r\n" + // Location of icon file.
                            "Type=Application\r\n" +
                            $"Categories=Application;{categoryContent}\r\n" + //Categories in which this app should be listed.
                            $"Comment={assemblyDescription}"; // Comment which appears as a tooltip.

                        File.WriteAllText(Path.Combine(shortcutFileLocation, assemblyName + GetShortcutFileType()), shortcutContent);
                    }
                    break;
                case OSPlatform.MacOS:
                    Logger.Error("Can't manage shortcut's for macOS");
                    return;
            }
        }

        /// <summary>
        /// Removes shortcut
        /// </summary>
        /// <param name="shortcutLocation">Where you want the shortcut to go.</param>
        /// <param name="folderName">The folder that will contain the shortcut.</param>
        public static void RemoveShortcut(ShortcutLocation shortcutLocation, string folderName = default)
        {
            if (Core.OS.OperatingSystem.OnMacOS)
            {
                Logger.Error("Can't manage shortcut's for macOS");
                return;
            }

            var shortcutDirectory = GetShortcutFileLocation(shortcutLocation);
            if (!string.IsNullOrWhiteSpace(folderName))
            {
                Logger.Debug($"Shortcut should be in '{folderName}'");
                shortcutDirectory = Path.Combine(shortcutDirectory, folderName);
            }

            var shortcutFileLocation = Path.Combine(shortcutDirectory, Assembly.GetEntryAssembly().GetName().Name + GetShortcutFileType());
            if (File.Exists(shortcutFileLocation))
            {
                Logger.Debug($"Shortcut is being '{folderName}'");
                File.Delete(shortcutFileLocation);
            }
            if (Directory.Exists(shortcutDirectory) && Directory.GetFiles(shortcutDirectory).Length == 0)
            {
                Logger.Debug($"Folder that contained the shortcut has nothing anymore, deleting '{folderName}'");
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
            if (Core.OS.OperatingSystem.OnMacOS)
            {
                Logger.Error("Can't manage shortcut's for macOS");
                return false;
            }
            
            var shortcutFileLocation = GetShortcutFileLocation(shortcutLocation);
            if (!string.IsNullOrWhiteSpace(folderName))
            {
                Logger.Debug($"Shortcut should be in '{folderName}'");
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
            var fileLocation = shortcutLocation == ShortcutLocation.StartMenu ?
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) :
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (shortcutLocation == ShortcutLocation.StartMenu && Core.OS.OperatingSystem.OnWindows)
            {
                if (Directory.Exists(Path.Combine(fileLocation, "Programs")))
                {
                    return Path.Combine(fileLocation, "Programs");
                }
            }
            return fileLocation;
        }

        /// <summary>
        /// Gets the file type the shortcut will be using
        /// </summary>
        /// <returns>Shortcut file type</returns>
        private static string GetShortcutFileType()
        {
            switch (Core.OS.OperatingSystem.OSPlatform)
            {
                case OSPlatform.Windows:
                    return WindowsFileType;
                case OSPlatform.Linux:
                    return LinuxFileType;
                case OSPlatform.MacOS:
                    Logger.Error("Can't manage shortcut's for macOS");
                    return "";
                default:
                    return "";
            }
        }
    }
}
