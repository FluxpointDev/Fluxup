using System;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;
using Fluxup.Core.Logging;
using System.Threading.Tasks;

namespace Fluxup.Core.Networking
{
    /// <summary>
    /// HttpClient extensions
    /// </summary>
    public static class HttpClientEx
    {
        private static readonly Logger Logger = new Logger(nameof(HttpClientEx));

        /// <summary>
        /// Gets a Stream from the internet
        /// </summary>
        /// <param name="httpClient">http client</param>
        /// <param name="requestUri">Uri to get a stream from</param>
        /// <returns>Stream from the internet</returns>
        public static async Task<Stream> GetStreamAsyncLogged(this HttpClient httpClient, string requestUri)
        {
            Logger.Debug($"Uri to grab stream from: {requestUri}");
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                Logger.Error("Internet is currently unavailable, can't continue");
                return Stream.Null;
            }

            try
            {
                var stream = await httpClient.GetStreamAsync(requestUri);
                if (stream == null)
                {
                    Logger.Error("Stream is null");
                    return Stream.Null;
                }
                if (!stream.CanRead)
                {
                    Logger.Warning("You can't read the stream, this is likely not going to be a usable stream");
                }
                return stream;
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return Stream.Null;
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
            Logger.Debug($"Uri to grab content from: {requestUri}");
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                Logger.Error("Internet is currently unavailable, can't continue");
                return default;
            }

            try
            {
                var re = await httpClient.GetAsync(requestUri);
                if (!re.IsSuccessStatusCode) 
                {
                    Logger.Warning($"{requestUri} gave unsuccessful status code");
                }
                return re;
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return default;
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
