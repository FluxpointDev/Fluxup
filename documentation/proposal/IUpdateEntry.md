# IUpdateEntry

```cs
public interface IUpdateEntry
{
    public string Filename { get; }
    public long Filesize { get; }
    public bool IsDelta { get; }
    public Task<string> FetchReleaseNote();
    public string SHA1 { get; }
    public Version Version { get; }
}
```
