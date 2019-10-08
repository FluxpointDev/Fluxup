using System;
using System.IO;
using System.Net.Http;
using Fluxup.Core.Logging;
using System.Threading.Tasks;

namespace Fluxup.Core.Networking
{
    /// <summary>
    /// HttpClient extensions
    /// </summary>
    public static class HttpClientEx
    {
        private static readonly Logger Logger = new Logger("Networking");

        /// <summary>
        /// Gets a Stream from the internet
        /// </summary>
        /// <param name="httpClient">http client</param>
        /// <param name="requestUri">Uri to get a stream from</param>
        /// <returns>Stream from the internet</returns>
        public static async Task<Stream> GetStreamAsyncLogged(this HttpClient httpClient, string requestUri)
        {
            Logger.Information($"Uri to grab stream from: {requestUri}");
            var stream = await httpClient.GetStreamAsync(requestUri);
            if (stream == null)
            {
                Logger.Warning("Stream is null");
                return null;
            }
            if (!stream.CanRead)
            {
                Logger.Warning("You can't read the stream, this is likely not going to be usable stream");
            }
            return stream;
        }

        /// <summary>
        /// Gets a Stream from the internet
        /// </summary>
        /// <param name="httpClient">http client</param>
        /// <param name="uri">Uri to get a stream from</param>
        /// <returns>Stream from the internet</returns>
        public static async Task<Stream> GetStreamAsyncLogged(this HttpClient httpClient, Uri uri)
        {
            return await GetStreamAsyncLogged(httpClient, uri.AbsoluteUri);
        }

        /// <summary>
        /// Gets a HttpResponseMessage from the internet
        /// </summary>
        /// <param name="httpClient">http client</param>
        /// <param name="requestUri">Uri to get a HttpResponseMessage from</param>
        /// <returns>HttpResponseMessage from the internet</returns>
        public static async Task<HttpResponseMessage> GetAsyncLogged(this HttpClient httpClient, string requestUri)
        {
            Logger.Information($"Uri to grab content from: {requestUri}");
            var re = await httpClient.GetAsync(requestUri);
            if (!re.IsSuccessStatusCode) 
            {
                Logger.Warning($"{requestUri} gave unsuccessful status code");
            }
            return re;
        }

        /// <summary>
        /// Gets a HttpResponseMessage from the internet
        /// </summary>
        /// <param name="httpClient">http client</param>
        /// <param name="uri">Uri to get a HttpResponseMessage from</param>
        /// <returns>HttpResponseMessage from the internet</returns>
        public static async Task<HttpResponseMessage> GetAsyncLogged(this HttpClient httpClient, Uri uri)
        {
            return await GetAsyncLogged(httpClient, uri.AbsoluteUri);
        }
    }
}
