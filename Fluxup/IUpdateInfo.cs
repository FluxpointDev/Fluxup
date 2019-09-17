using System;
using System.Threading.Tasks;

namespace Fluxup.Updater
{
    public interface IUpdateInfo
    {
        IUpdateEntry[] Updates { get; }
        Version NewestUpdateVersion { get; }
        Task<string[]> FetchReleaseNotes();
    }
}
