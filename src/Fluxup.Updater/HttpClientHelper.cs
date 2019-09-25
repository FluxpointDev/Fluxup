using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Fluxup.Updater
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
