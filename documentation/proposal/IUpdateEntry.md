# IUpdateEntry

```cs
public interface IUpdateEntry
{
    string Filename { get; }
    long Filesize { get; }
    bool IsDelta { get; }
    Task<string> FetchReleaseNote();
    string SHA1 { get; }
    Version Version { get; }
}
```
