# IUpdateInfo

```cs
public interface IUpdateInfo
{
    UpdateEntry[] Updates { get; }
    Version NewestUpdateVersion { get; }
    Task<string[]> FetchReleaseNotes();
}
```
