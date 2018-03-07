using System.Net.Http;
using System.Threading.Tasks;

namespace ProxyServer.Http
{
    public interface IProxyHttpClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption);
    }
}
