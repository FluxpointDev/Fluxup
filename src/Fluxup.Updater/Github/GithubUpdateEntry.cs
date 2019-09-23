using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fluxup.Updater.Github
{
    public class GithubUpdateEntry : IUpdateEntry
    {
        public string Filename => throw new NotImplementedException();

        public long Filesize => throw new NotImplementedException();

        public bool IsDelta => throw new NotImplementedException();

        public string SHA1 => throw new NotImplementedException();

        public Version Version => throw new NotImplementedException();

        public Task<string> FetchReleaseNote()
        {
            throw new NotImplementedException();
        }
    }
}
