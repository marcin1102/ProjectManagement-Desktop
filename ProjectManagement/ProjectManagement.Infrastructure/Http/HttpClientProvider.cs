﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManagement.Infrastructure.Http
{
    public static class HttpClientProvider
    {
        internal static HttpClient HttpClient = new HttpClient()
        {
            BaseAddress = new Uri("https://projectmanagementbackend.azurewebsites.net/")
        };
    }
}
