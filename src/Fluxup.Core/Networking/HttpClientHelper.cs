using System.Net.Http;

namespace Fluxup.Core.Networking
{
    /// <summary>
    /// Helps make a HttpClient
    /// </summary>
    public static class HttpClientHelper
    {
        /// <summary>
        /// Makes a HttpClient with set settings.
        /// </summary>
        /// <param name="applicationName">The application name</param>
        /// <returns>HttpClient</returns>
        public static HttpClient CreateHttpClient(string applicationName)
        {
            var httpClient = HttpClientFactory.Create();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"Fluxup-{applicationName}");
            return httpClient;
        }
    }
}
