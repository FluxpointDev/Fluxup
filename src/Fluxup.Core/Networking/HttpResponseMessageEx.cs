using System.Net.Http;

namespace Fluxup.Core.Networking
{
    internal static class HttpResponseMessageEx
    {
        public static string ErrorResponseMessage(this HttpResponseMessage httpResponseMessage)
        {
            return httpResponseMessage != null ? $"\r\n  Status Code: {httpResponseMessage.StatusCode}" +
                    $"\r\n  Reason Phrase: {httpResponseMessage.ReasonPhrase}"
                    : "";
        }
    }
}
