using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProxyServer.Tests.Mock
{
    public class HttpContentMock: HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return Task.FromResult("");
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return true;
        }
    }
}
