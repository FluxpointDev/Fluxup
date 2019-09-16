# IUpdateInfo

```cs
public interface IUpdateInfo
{
    public UpdateEntry[] Updates { get; }
    public Version NewestUpdateVersion { get; }
    public Task<string[]> FetchReleaseNotes();
}
```
