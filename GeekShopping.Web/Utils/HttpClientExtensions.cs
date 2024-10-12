using System.Net.Http.Headers;
using System.Text.Json;

namespace GeekShopping.Web.Utils
{
    public static class HttpClientExtensions
    {
        private static MediaTypeHeaderValue _contentType =
            new MediaTypeHeaderValue("application/json");

        private static readonly JsonSerializerOptions _jsonSerializerOptions =
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public static async Task<T> ReadContentAs<T>(
            this HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error: {response.ReasonPhrase}");
            }

            var dataAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonSerializer.Deserialize<T>(dataAsString, _jsonSerializerOptions);
        }

        public static async Task<HttpResponseMessage> PostAsJson<T>(
            this HttpClient client, 
            string requestUri, 
            T data)
        {
            var dataAsString = JsonSerializer.Serialize(data);
            var content = new StringContent(dataAsString);
            content.Headers.ContentType = _contentType;

            return await client.PostAsync(requestUri, content);
        }

        public static async Task<HttpResponseMessage> PutAsJson<T>(
            this HttpClient client,
            string requestUri,
            T data)
        {
            var dataAsString = JsonSerializer.Serialize(data);
            var content = new StringContent(dataAsString);
            content.Headers.ContentType = _contentType;

            return await client.PutAsync(requestUri, content);
        }


    }
}
