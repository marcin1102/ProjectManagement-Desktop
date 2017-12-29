using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManagement.Infrastructure.Http
{
    public static class HttpClientProvider
    {
        internal static HttpClient HttpClient;

        public static void SetHttpClientUri(bool isLocal)
        {
            string uri;
            if (isLocal)
                uri = "http://localhost:5000/";
            else
                uri = "https://projectmanagementbackend.azurewebsites.net/";

            HttpClient = new HttpClient()
            {
                BaseAddress = new Uri(uri)
            };
        }
    }
}
