using System;
using System.IO;
using System.Net.Http;
using Fluxup.Core.Logging;
using System.Threading.Tasks;

namespace Fluxup.Core.Networking
{
    public static class HttpClientEx
    {
        private static Logger Logger = new Logger("Networking");

        public static async Task<Stream> GetStreamAsyncLogged(this HttpClient httpClient, string requestUri)
        {
            Logger.Information($"Uri to grab stream from: {requestUri}");
            var stream = await httpClient.GetStreamAsync(requestUri);
            if (stream == null)
            {
                Logger.Warning("Stream is null");
            }
            if (!stream.CanRead)
            {
                Logger.Warning("You can't read the stream, this is likely not going to be useable stream");
            }
            return stream;
        }

        public static async Task<Stream> GetStreamAsyncLogged(this HttpClient httpClient, Uri uri)
        {
            return await GetStreamAsyncLogged(httpClient, uri.AbsoluteUri);
        }

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

        public static async Task<HttpResponseMessage> GetAsyncLogged(this HttpClient httpClient, Uri uri)
        {
            return await GetAsyncLogged(httpClient, uri.AbsoluteUri);
        }
    }
}
