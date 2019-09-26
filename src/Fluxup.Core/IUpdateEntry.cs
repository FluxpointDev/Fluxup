using System;
using System.Threading.Tasks;

namespace Fluxup.Core
{
    public interface IUpdateEntry
    {
        string Filename { get; }
        long Filesize { get; }
        bool IsDelta { get; }
        string SHA1 { get; }
        Version Version { get; }

        Task<string> FetchReleaseNote();
    }
}
