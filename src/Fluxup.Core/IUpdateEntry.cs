using SemVersion;
using System.Threading.Tasks;

namespace Fluxup.Core
{
    public interface IUpdateEntry
    {
        string Filename { get; }
        long Filesize { get; }
        bool IsDelta { get; }
        string SHA1 { get; }
        SemanticVersion Version { get; }

        Task<string> FetchReleaseNote();
    }
}
