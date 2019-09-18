using System;
using System.Reflection;
using System.Threading.Tasks;
using Fluxup.Updater;
using Fluxup.Updater.Github;

namespace Fluxup.Example
{ 
    public static class Program
    {
        private static async Task Main(string[] args)
        {
            ShortcutManager.CreateShortcut(ShortcutLocation.Desktop, new []{ ApplicationCategory.Development, ApplicationCategory.ConsoleOnly });
            
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