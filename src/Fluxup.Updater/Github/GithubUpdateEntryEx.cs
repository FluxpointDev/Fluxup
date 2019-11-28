using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Fluxup.Updater.Github
{
    /// <summary>
    /// Extensions for <see cref="Fluxup.Updater.Github.GithubUpdateEntry"/>
    /// </summary>
    public static class GithubUpdateEntryEx
    {
        /// <summary>
        /// Checks the hash with the stream of a file
        /// </summary>
        /// <param name="updateEntry">updateEntry</param>
        /// <param name="fileStream">Stream of a file</param>
        /// <returns>If the hash is the same as the file hash</returns>
        public static bool CheckHash(this GithubUpdateEntry updateEntry, Stream fileStream, out string computedHash)
        {
            return fileStream.CheckHash(updateEntry.SHA1, out computedHash);
        }
    }
}