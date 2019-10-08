using System.Net.Http;

namespace Fluxup.Core.Networking
{
    /// <summary>
    /// HttpResponseMessage extensions
    /// </summary>
    public static class HttpResponseMessageEx
    {
        /// <summary>
        /// Returns a error message from a HttpResponseMessage
        /// </summary>
        /// <param name="httpResponseMessage">HttpResponseMessage</param>
        /// <returns>HttpResponseMessage</returns>
        public static string ErrorResponseMessage(this HttpResponseMessage httpResponseMessage)
        {
            return httpResponseMessage != null && !httpResponseMessage.IsSuccessStatusCode ? 
                $"\r\n  Status Code: {httpResponseMessage.StatusCode}" +
                $"\r\n  Reason Phrase: {httpResponseMessage.ReasonPhrase}" :
                "";
        }
    }
}
