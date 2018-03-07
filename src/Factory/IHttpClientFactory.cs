using ProxyServer.Http;
using ProxyServer.Model;

namespace ProxyServer.Factory
{
    public interface IHttpClientFactory
    {
        IProxyHttpClient Create(HttpClientType type);
    }
}
