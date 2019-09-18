using System;
using System.Threading.Tasks;

namespace Fluxup.Updater
{
    public interface IUpdateInfo<TUpdateEntry>
    where TUpdateEntry : IUpdateEntry
    {
        bool HasUpdate { get; }
        TUpdateEntry[] Updates { get; }
        Version NewestUpdateVersion { get; }
        
        Task<string[]> FetchReleaseNotes();
    }
}
