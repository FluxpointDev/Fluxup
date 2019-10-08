namespace Fluxup.Updater.Github
{
    /// <summary>
    /// Extensions for <see cref="Fluxup.Updater.Github.GithubUpdateFetcher"/>
    /// </summary>
    public static class GithubUpdateFetcherEx
    {
        internal static bool AddVersionAndDeltaFromFileName(this GithubUpdateEntry updateEntry, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !fileName.Contains("-") || !fileName.Contains("."))
            {
                return false;
            }

            var index = fileName.IndexOf("-");
            var lastIndex = fileName.LastIndexOf("-");
            if (lastIndex == index)
            {
                return false;
            }

            var delta = fileName.Remove(0, lastIndex + 1);
            delta = delta.Remove(delta.IndexOf("."));
            switch (delta)
            {
                case "delta":
                    updateEntry.IsDelta = true;
                    break;
                case "full":
                    updateEntry.IsDelta = false;
                    break;
                default:
                    return false;
            }
            if (!SemVersion.SemanticVersion.TryParse(fileName.Remove(0, index + 1)
                .Remove(lastIndex - index - 1), out var version))
            {
                return false;
            }

            updateEntry.Version = version;
            return true;

        }
    }
}
