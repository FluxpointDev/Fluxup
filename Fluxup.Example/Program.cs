using System;
using System.Reflection;
using System.Threading.Tasks;
using Fluxup.Updater.Logging;
using Fluxup.Updater.Github;
using Fluxup.Updater.Manager;
using Fluxup.Updater;

namespace Fluxup.Example
{ 
    public static class Program
    {
        private static Logger Logger = new Logger("Example Application");

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

            var updateFetcher = new GithubUpdateFetcher("FluxupExample", "FluxpointDev", "FluxupExample");
            var info = await updateFetcher.CheckForUpdate();
            if (info.HasUpdate)
            {
                Console.WriteLine("They is a update, will do it in the background...");
                Task.Run(async () =>
                {
                    await updateFetcher.DownloadUpdates(info.Updates, 
                        d => Console.WriteLine($"Update download progress is {d}%"),
                        e =>
                        {
                            Console.WriteLine("Update download failed :(");
                            return;
                        });
                    await updateFetcher.InstallUpdates(info.Updates, 
                        d => Console.WriteLine($"Update install progress is {d}%"),
                        e =>
                        {
                            Console.WriteLine("Update download failed :(");
                            return;
                        });
                    Console.WriteLine("Update has downloaded and installed, restart app to apply update");
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