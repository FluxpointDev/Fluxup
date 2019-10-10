using System;
using System.Reflection;
using System.Threading.Tasks;
using Fluxup.Updater.Github;
using Fluxup.Updater.Manager;
using Fluxup.Updater;
using Fluxup.Core.Logging;

namespace Fluxup.Example
{ 
    public static class Program
    {
        private static Logger Logger = new Logger("Example Application");
        private static GithubUpdateFetcher updateFetcher = new GithubUpdateFetcher("Fluxup", "ppy", "osu");

        private static async Task Main(string[] args)
        {
            Logger.NewLog += (sender, logArgs) => Console.WriteLine($"[{logArgs.LoggerName} - {logArgs.LogLevel}]: {logArgs.Message}");
            ShortcutManager.CreateShortcut(ShortcutLocation.Desktop, "", applicationCategories: new []{ ApplicationCategory.Development, ApplicationCategory.ConsoleOnly });
            Logger.Debug($"Does shortcut exist?: {ShortcutManager.DoesShortcutExist(ShortcutLocation.Desktop)}");
            ShortcutManager.RemoveShortcut(ShortcutLocation.Desktop);
            Logger.Debug($"Does shortcut exist?: {ShortcutManager.DoesShortcutExist(ShortcutLocation.Desktop)}");

            ShortcutManager.CreateShortcut(ShortcutLocation.StartMenu, "", applicationCategories: new[] { ApplicationCategory.Development, ApplicationCategory.ConsoleOnly });
            Logger.Debug($"Does shortcut exist?: {ShortcutManager.DoesShortcutExist(ShortcutLocation.StartMenu)}");
            ShortcutManager.RemoveShortcut(ShortcutLocation.StartMenu);
            Logger.Debug($"Does shortcut exist?: {ShortcutManager.DoesShortcutExist(ShortcutLocation.StartMenu)}");

            var info = await updateFetcher.CheckForUpdate();
            if (info.HasUpdate)
            {
                Console.WriteLine("An update is available, will do it in the background...");
                _ = Task.Run(async () =>
                {
                    await updateFetcher.DownloadUpdates(info.Updates, 
                        d => Console.WriteLine($"Update download progress: {d}%"),
                        e =>
                        {
                            Console.WriteLine($"Update download failed :(\r\n{e}");
                        });
                    await updateFetcher.InstallUpdates(info.Updates, 
                        d => Console.WriteLine($"Update install progress: {d}%"),
                        e =>
                        {
                            Console.WriteLine($"Update install failed :(\r\n{e}");
                        });
                    Console.WriteLine("Update has been downloaded and installed, restart app to apply update");
                });
            }
            
            Console.WriteLine($"Hello World! App version: {Assembly.GetExecutingAssembly().GetName().Version}");
            do
            {
                Console.Write("> ");
                if (Console.ReadLine() == "exit")
                {
                    break;
                }
            } while (true);
        }
    }
}