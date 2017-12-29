using ProjectManagement.Infrastructure.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ProjectManagement.Infrastructure.Primitives.Message;

namespace ProjectManagement.Infrastructure.Message
{
    public class CommandQueryDispatcher
    {
        private string accessTokenHeaderName = "AccessToken";

        public async Task<(HttpStatusCode StatusCode, string ResponseContent)> SendAsync<TCommand>(TCommand command, string uri, HttpOperationType httpOperationType)
            where TCommand : class, ICommand
        {
            HttpResponseMessage response;
            var content = new StringContent(JsonConvert.SerializeObject(command));

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
            if (uri.Contains("login") && response.StatusCode == HttpStatusCode.OK)
            {
                HttpClientProvider.HttpClient.DefaultRequestHeaders.Add(accessTokenHeaderName, responseContent.Replace("token: ", ""));
            }

            return (response.StatusCode, responseContent);
        }

        public async Task<(HttpStatusCode StatusCode, TResponse ResponseContent)> SendAsync<TResponse>(string uri)
            where TResponse : class
        {
            var response = await HttpClientProvider.HttpClient.GetAsync(uri);
            if(!response.IsSuccessStatusCode)
                return (response.StatusCode, null);

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

        public void RemoveAccessToken()
        {
            HttpClientProvider.HttpClient.DefaultRequestHeaders.Remove(accessTokenHeaderName);
        }
    }
}
