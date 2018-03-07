using ProxyServer.Model;

namespace ProxyServer.Tests.Mock
{
    public static class ProxyOptionsMockExtensions
    {
        public static void WithHost(this ProxyOptionsMock instance, string host)
        {
            instance.Host = host;
        }
    }

    public class ProxyOptionsMock: ProxyOptions
    {
        public ProxyOptionsMock()
        {
            Host = "localhost";
            Port = "80";
            Scheme = "http";
        }
    }
}
