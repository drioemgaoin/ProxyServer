using System;
using System.Net.Http;
using ProxyServer.Http;
using ProxyServer.Model;

namespace ProxyServer.Factory
{
    public class HttpClientFactory: IHttpClientFactory
    {
        public IProxyHttpClient Create(HttpClientType type)
        {
            switch (type)
            {
                case HttpClientType.HttpClient:
                    return new ProxyHttpClient(new HttpClientHandler());
            }

            throw new Exception($"{type} is an unknown http client type");
        }
    }
}
