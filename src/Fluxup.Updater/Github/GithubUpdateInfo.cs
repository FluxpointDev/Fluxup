using Fluxup.Core;
using SemVersion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fluxup.Updater.Github
{
    /// <summary>
    /// Updates from Github
    /// </summary>
    public class GithubUpdateInfo : IUpdateInfo<GithubUpdateEntry>
    {
        internal GithubUpdateInfo(IEnumerable<GithubUpdateEntry> updates)
        {
            //Filter out any updates that are null for now...
            Updates = updates?.Where(x => x != null).ToArray();
            NewestUpdateVersion = Updates?.FirstOrDefault()?.Version;
            HasUpdate = Core.HasUpdate.ApplicationHasUpdate(NewestUpdateVersion);
        }

        /// <inheritdoc cref="Fluxup.Core.IUpdateInfo{TUpdateEntry}.HasUpdate"/>
        public bool HasUpdate { get; }

        /// <inheritdoc cref="Fluxup.Core.IUpdateInfo{TUpdateEntry}.Updates"/>
        public GithubUpdateEntry[] Updates { get; }

        /// <inheritdoc cref="Fluxup.Core.IUpdateInfo{TUpdateEntry}.NewestUpdateVersion"/>
        public SemanticVersion NewestUpdateVersion { get; }

        //TODO: Add this...
        /// <inheritdoc cref="Fluxup.Core.IUpdateInfo{TUpdateEntry}.UpdateRequired"/>
        public bool UpdateRequired => throw new NotImplementedException();

        /// <inheritdoc cref="Fluxup.Core.IUpdateInfo{TUpdateEntry}.FetchReleaseNotes()"/>
        public async Task<string[]> FetchReleaseNotes()
        {
            var count = Updates.LongCount();
            var releaseNotes = new string[count];
            for (var i = 0; i < count; i++)
            {
                releaseNotes[i] = await Updates[i].FetchReleaseNote() ?? $"No release note for update {Updates[i].Version}.";
            }

            return releaseNotes;
        }
    }
}
