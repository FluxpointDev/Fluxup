# IUpdateFetcher

```cs
public interface IUpdateFetcher
{
    string ApplicationName { get; }
    string IsInstalledApp { get; }
   
    Task<UpdateInfo> CheckForUpdate(bool useDeltaPatching = true);
    Task DownloadUpdates(UpdateEntry[] updateEntry, Action<double> progress);
    Task DownloadUpdates(UpdateInfo updateinfo, Action<double> progress);
    Task InstallUpdates(UpdateInfo updateinfo, Action<double> progress);
}
```
