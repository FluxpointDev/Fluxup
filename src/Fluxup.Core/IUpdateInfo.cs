using System;
using System.Threading.Tasks;

namespace Fluxup.Core
{
    public interface IUpdateInfo<TUpdateEntry>
    where TUpdateEntry : IUpdateEntry
    {
        bool HasUpdate { get; }
        bool UpdateRequired { get; }
        TUpdateEntry[] Updates { get; }
        Version NewestUpdateVersion { get; }
        
        Task<string[]> FetchReleaseNotes();
    }
}
