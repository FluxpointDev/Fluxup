using System.Net.Http;

namespace Fluxup.Core.Networking
{
    public static class HttpClientHelper
    {
        public static HttpClient CreateHttpClient(string applicationName)
        {
            var httpClient = HttpClientFactory.Create();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"Fluxup-{applicationName}");
            return httpClient;
        }
    }
}
