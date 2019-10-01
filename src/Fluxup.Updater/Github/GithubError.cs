using System;
using Newtonsoft.Json;

namespace Fluxup.Updater.Github
{
    internal class GithubError
    {
        [JsonProperty("message")]
        public string Message { get; private set; }

        [JsonProperty("documentation_url")]
        public Uri DocumentationUrl { get; private set; }
    }
}