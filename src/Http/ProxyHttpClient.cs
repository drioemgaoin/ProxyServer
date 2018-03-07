using System.Net.Http;

namespace ProxyServer.Http
{
    public class ProxyHttpClient: HttpClient, IProxyHttpClient
    {
        public ProxyHttpClient(HttpMessageHandler handler)
            : base(handler)
        {
        }
    }
}
