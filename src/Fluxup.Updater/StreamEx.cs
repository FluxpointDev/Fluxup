using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Fluxup.Updater
{
    public static class StreamEx
    {
        /// <summary>
        /// Checks the hash of the stream
        /// </summary>
        /// <param name="stream">stream</param>
        /// <param name="exceptedHash">Hash that we are excepting</param>
        /// <param name="computedHash">Hash that we get</param>
        /// <returns>If the <see cref="exceptedHash"/> is the same as <see cref="computedHash"/></returns>
        public static bool CheckHash(this Stream stream, string exceptedHash, out string computedHash, bool disposeStreamAfter = true)
        {
            using var sha1 = SHA1.Create();
            
            var hashByte = sha1.ComputeHash(stream);
            if (disposeStreamAfter)
            {
                stream.Dispose();
            }
            
            computedHash = hashByte.Aggregate("", (current, b) => current + b.ToString("X"));
            
            return computedHash == exceptedHash;

        }
    }
}