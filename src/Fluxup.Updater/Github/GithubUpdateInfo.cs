using Fluxup.Core;
using SemVersion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fluxup.Updater.Github
{
    public class GithubUpdateInfo : IUpdateInfo<GithubUpdateEntry>
    {
        internal GithubUpdateInfo(IEnumerable<GithubUpdateEntry> updates)
        {
            //Filter out any updates that are null for now...
            Updates = updates.Where(x => x != null).ToArray();
            NewestUpdateVersion = Updates?.FirstOrDefault()?.Version;
            HasUpdate = Updates != null && Updates.LongCount() > 0;
        }

        public bool HasUpdate { get; }

        public GithubUpdateEntry[] Updates { get; }

        public SemanticVersion NewestUpdateVersion { get; }

        public bool UpdateRequired => throw new NotImplementedException();

        public async Task<string[]> FetchReleaseNotes()
        {
            var count = Updates.LongCount();
            var releaseNotes = new string[count];
            for (int i = 0; i < count; i++)
            {
                releaseNotes[i] = await Updates[i].FetchReleaseNote() ?? $"No release note for update {Updates[i].Version}.";
            }

            return releaseNotes;
        }
    }
}
