using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fluxup.Updater.Github
{
    class GithubUpdateInfo : IUpdateInfo
    {
        public IUpdateEntry[] Updates => throw new NotImplementedException();

        public Version NewestUpdateVersion => throw new NotImplementedException();

        public Task<string[]> FetchReleaseNotes()
        {
            throw new NotImplementedException();
        }
    }
}
