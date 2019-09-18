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
        /// <summary>
        /// Makes a shortcut to the application.
        /// </summary>
        /// <param name="shortcutLocation">Where you want the shortcut to go.</param>
        /// <param name="applicationCategories">Categories that the application fit into</param>
        /// <param name="folderName">The folder that will contain the shortcut</param>
        /// <param name="linuxUiLib">The GUI lib that the application using if being made *for* linux</param>
        /// <exception cref="NotImplementedException">Windows and macOS is not added (yet)</exception>
        public static void CreateShortcut(ShortcutLocation shortcutLocation, ApplicationCategory[] applicationCategories, string folderName = default, LinuxUILib linuxUiLib = LinuxUILib.None)
        {
            var shortcutContent = "";
            var shortcutFileType = "";
            var shortcutFileLocation = shortcutLocation == ShortcutLocation.StartMenu ? 
                    Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) :
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            var assembly = Assembly.GetEntryAssembly();
            var assemblyName = assembly.GetName().Name;
            switch (OperatingSystem.GetOSPlatform())
            {
                case OSPlatform.Windows:
                    throw new NotImplementedException();
                case OSPlatform.Linux:
                {
                    var categoryContent = "";
                    var catCount = applicationCategories.Length;
                    var isTerminalApp = false;
                    var doneAudioVideoCheck = false;
                    for (int i = 0; i < catCount; i++)
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
                            //TODO: Add code for application categories that need another application Categories
                        }
                    }

                    if (linuxUiLib != LinuxUILib.None)
                    {
                        categoryContent += linuxUiLib + ";";
                    }
                    if (!applicationCategories.Contains(ApplicationCategory.AudioVideo) && 
                        applicationCategories.Contains(ApplicationCategory.Video) && 
                        applicationCategories.Contains(ApplicationCategory.Audio))
                    {
                        categoryContent += "AudioVideo;";
                    }

                    shortcutFileType = ".desktop";
                    shortcutContent =
                        "[Desktop Entry]\r\n" +
                        "Encoding=UTF-8\r\n" +
                        $"Name={assemblyName}\r\n" + //name of an app.
                        $"Exec=./'{assembly.Location.Replace(".dll", "")}'    \r\n" + //command used to launch an app. //TODO: Fix it not load the application
                        $"Terminal={isTerminalApp}\r\n" + //whether an app requires to be run in a terminal.
                        $"Icon={assembly.Location}\r\n" + //location of icon file.
                        "Type=Application\r\n" + //type
                        $"Categories=Application;{categoryContent}\r\n" + //categories in which this app should be listed.
                        $"Comment={assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()}"; //comment which appears as a tooltip.
                }
                break;
                case OSPlatform.MacOS:
                    throw new NotImplementedException();
            }

            if (!string.IsNullOrWhiteSpace(folderName))
            {
                shortcutFileLocation = Path.Combine(shortcutFileLocation, folderName);
                if (!Directory.Exists(shortcutFileLocation))
                {
                    Directory.CreateDirectory(shortcutFileLocation);
                }
            }
            File.WriteAllText(Path.Combine(shortcutFileLocation, assemblyName + shortcutFileType), shortcutContent);
        }

        public static void RemoveShortcut(ShortcutLocation shortcutLocation, string folderName = default)
        {
            throw new NotImplementedException();
        }
    }
}
