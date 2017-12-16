using Infrastructure.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Infrastructure.Message
{
    public class CommandQueryDispatcher
    {
        private readonly string host = "http://localhost/api";
        private string accessToken;
        
        public async Task<(HttpStatusCode, string)> SendAsync<TCommand>(TCommand command, string uri, HttpOperationType httpOperationType)
            where TCommand : class
        {
            HttpResponseMessage response;
            var content = new StringContent(JsonConvert.SerializeObject(command));

            content.Headers.Add("AccessToken", accessToken);
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");


            switch (httpOperationType)
            {
                case HttpOperationType.POST:
                    response = await HttpClientProvider.HttpClient.PostAsync(uri, content);
                    break;
                case HttpOperationType.PUT:
                    response = await HttpClientProvider.HttpClient.PutAsync(uri, content);
                    break;
                case HttpOperationType.PATCH:
                    response = await PatchAsync(HttpClientProvider.HttpClient, uri, content);
                    break;
                default:
                    throw new InvalidOperationException($"Cannot execute operation with type: {httpOperationType.ToString()}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            if (uri.Contains("login"))
                accessToken = responseContent.Replace("token: ", "");

            return (response.StatusCode, responseContent);
        }

        public async Task<(HttpStatusCode statusCode, TResponse response)> SendAsync<TQuery, TResponse>(TQuery command, string uri)
            where TQuery : class
        {
            var content = new StringContent(JsonConvert.SerializeObject(command));
            content.Headers.Add("AccessToken", accessToken);
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

            var response = await HttpClientProvider.HttpClient.PostAsync(uri, content);
            var responseBody = JsonConvert.DeserializeObject<TResponse>(await response.Content.ReadAsStringAsync());

            return (response.StatusCode, responseBody);
        }

        private async Task<HttpResponseMessage> PatchAsync(HttpClient client, string requestUri, HttpContent content)
        {
            var method = new HttpMethod("PATCH");

            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = content
            };

            HttpResponseMessage response = new HttpResponseMessage();
            // In case you want to set a timeout
            //CancellationToken cancellationToken = new CancellationTokenSource(60).Token;
            response = await client.SendAsync(request);
            return response;
        }
    }
}
