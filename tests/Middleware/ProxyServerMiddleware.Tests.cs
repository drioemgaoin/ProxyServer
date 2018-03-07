using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using ProxyServer.Factory;
using ProxyServer.Http;
using ProxyServer.Middleware;
using ProxyServer.Model;
using ProxyServer.Tests.Mock;

namespace ProxyServer.Tests.Middleware
{
    [TestFixture]
    public class ProxyServerMiddlewareTests
    {
        private IFixture fixture;
        private Mock<IOptions<ProxyOptions>> optionsMock;
        private Mock<IHttpClientFactory> httpClientFactoryMock;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            fixture = new Fixture().Customize(new AutoMoqCustomization());

            optionsMock = fixture.Freeze<Mock<IOptions<ProxyOptions>>>();
            httpClientFactoryMock = fixture.Freeze<Mock<IHttpClientFactory>>();
        }

        [TearDown]
        public void TearDown()
        {
            optionsMock.Reset();
            httpClientFactoryMock.Reset();
        }

        [Test]
        public void Invoke_WhenSchemeIsSetted_ShouldUseTheSettedScheme()
        {
            // Arrange
            var requestDelegate = fixture.Create<RequestDelegate>();
            var httpClientMock = fixture.Freeze<Mock<IProxyHttpClient>>();

            var options = CreateProxyOptions();
            options.Scheme = "https";

            optionsMock.Setup(x => x.Value).Returns(options);

            httpClientFactoryMock.Setup(x => x.Create(HttpClientType.HttpClient))
                .Returns(httpClientMock.Object);

            var subject = new ProxyServerMiddleware(httpClientFactoryMock.Object, requestDelegate, optionsMock.Object);

            // Action
            subject.Invoke(new HttpContextMock()).Wait();

            // Assets
            httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(p => p.RequestUri.Scheme == "https"), HttpCompletionOption.ResponseHeadersRead));
        }

        [Test]
        public void Invoke_WhenSchemeIsNotSetted_ShouldUseHttpScheme()
        {
            // Arrange
            var requestDelegate = fixture.Create<RequestDelegate>();
            var httpClientMock = fixture.Freeze<Mock<IProxyHttpClient>>();

            var options = CreateProxyOptions();
            options.Scheme = null;

            optionsMock.Setup(x => x.Value).Returns(options);

            httpClientFactoryMock.Setup(x => x.Create(HttpClientType.HttpClient))
                .Returns(httpClientMock.Object);

            var subject = new ProxyServerMiddleware(httpClientFactoryMock.Object, requestDelegate, optionsMock.Object);

            // Action
            subject.Invoke(new HttpContextMock()).Wait();

            // Assets
            httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(p => p.RequestUri.Scheme == "http"), HttpCompletionOption.ResponseHeadersRead));
        }

        [Test]
        public void Invoke_WhenHostIsSetted_ShouldUseTheSettedHost()
        {
            // Arrange
            var requestDelegate = fixture.Create<RequestDelegate>();
            var httpClientMock = fixture.Freeze<Mock<IProxyHttpClient>>();

            var options = CreateProxyOptions();
            options.Host = "mycustomhost";
            
            optionsMock.Setup(x => x.Value).Returns(options);

            httpClientFactoryMock.Setup(x => x.Create(HttpClientType.HttpClient))
                .Returns(httpClientMock.Object);

            var subject = new ProxyServerMiddleware(httpClientFactoryMock.Object, requestDelegate, optionsMock.Object);

            // Action
            subject.Invoke(new HttpContextMock()).Wait();

            // Assets
            httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(p => p.RequestUri.Host == "mycustomhost"), HttpCompletionOption.ResponseHeadersRead));
        }

        [Test]
        public void Invoke_WhenHostIsNotSetted_ShouldThrowException()
        {
            // Arrange
            var requestDelegate = fixture.Create<RequestDelegate>();
            var httpClientMock = fixture.Freeze<Mock<IProxyHttpClient>>();

            var options = CreateProxyOptions();
            options.Host = null;

            optionsMock.Setup(x => x.Value).Returns(options);

            httpClientFactoryMock.Setup(x => x.Create(HttpClientType.HttpClient))
                .Returns(httpClientMock.Object);

            TestDelegate action = () => new ProxyServerMiddleware(httpClientFactoryMock.Object, requestDelegate, optionsMock.Object);

            // Action

            // Assets
            var exception = Assert.Throws<ArgumentException>(action);
            Assert.AreEqual("Host is undefined", exception.Message);
        }

        [Test]
        public void Invoke_WhenPortIsSetted_ShouldUseTheSettedPort()
        {
            // Arrange
            var requestDelegate = fixture.Create<RequestDelegate>();
            var httpClientMock = fixture.Freeze<Mock<IProxyHttpClient>>();

            var options = CreateProxyOptions();
            options.Port = "81";

            optionsMock.Setup(x => x.Value).Returns(options);

            httpClientFactoryMock.Setup(x => x.Create(HttpClientType.HttpClient))
                .Returns(httpClientMock.Object);

            var subject = new ProxyServerMiddleware(httpClientFactoryMock.Object, requestDelegate, optionsMock.Object);

            // Action
            subject.Invoke(new HttpContextMock()).Wait();

            // Assets
            httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(p => p.RequestUri.Port == 81), HttpCompletionOption.ResponseHeadersRead));
        }

        [Test]
        public void Invoke_WhenPortIsNotSettedAndSchemeIsHttp_ShouldUsePort80()
        {
            // Arrange
            var requestDelegate = fixture.Create<RequestDelegate>();
            var httpClientMock = fixture.Freeze<Mock<IProxyHttpClient>>();

            var options = CreateProxyOptions();
            options.Port = null;

            optionsMock.Setup(x => x.Value).Returns(options);

            httpClientFactoryMock.Setup(x => x.Create(HttpClientType.HttpClient))
                .Returns(httpClientMock.Object);

            var subject = new ProxyServerMiddleware(httpClientFactoryMock.Object, requestDelegate, optionsMock.Object);

            // Action
            subject.Invoke(new HttpContextMock()).Wait();

            // Assets
            httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(p => p.RequestUri.Port == 80), HttpCompletionOption.ResponseHeadersRead));
        }

        [Test]
        public void Invoke_WhenPortIsNotSettedAndSchemeIsHttps_ShouldUsePort443()
        {
            // Arrange
            var requestDelegate = fixture.Create<RequestDelegate>();
            var httpClientMock = fixture.Freeze<Mock<IProxyHttpClient>>();

            var options = CreateProxyOptions();
            options.Port = null;
            options.Scheme = "https";

            optionsMock.Setup(x => x.Value).Returns(options);

            httpClientFactoryMock.Setup(x => x.Create(HttpClientType.HttpClient))
                .Returns(httpClientMock.Object);

            var subject = new ProxyServerMiddleware(httpClientFactoryMock.Object, requestDelegate, optionsMock.Object);

            // Action
            subject.Invoke(new HttpContextMock()).Wait();

            // Assets
            httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(p => p.RequestUri.Port == 443), HttpCompletionOption.ResponseHeadersRead));
        }

        [Test]
        public void Invoke_WhenLocalPathIsDefined_ShouldUseTheSettedLocalPath()
        {
            // Arrange
            var requestDelegate = fixture.Create<RequestDelegate>();
            var httpClientMock = fixture.Freeze<Mock<IProxyHttpClient>>();

            var options = CreateProxyOptions();
            options.PrefixType = PathPrefixType.Defined;
            options.DefinedPrefix = "/mycustomprefix";

            optionsMock.Setup(x => x.Value).Returns(options);

            httpClientFactoryMock.Setup(x => x.Create(HttpClientType.HttpClient))
                .Returns(httpClientMock.Object);

            var subject = new ProxyServerMiddleware(httpClientFactoryMock.Object, requestDelegate, optionsMock.Object);

            // Action
            subject.Invoke(new HttpContextMock()).Wait();

            // Assets
            httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(p => p.RequestUri.LocalPath == "/mycustomprefix"), HttpCompletionOption.ResponseHeadersRead));
        }

        [Test]
        public void Invoke_WhenLocalPathIsLocal_ShouldUseTheSettedLocalPath()
        {
            // Arrange
            var requestDelegate = fixture.Create<RequestDelegate>();
            var httpClientMock = fixture.Freeze<Mock<IProxyHttpClient>>();

            var options = CreateProxyOptions();
            options.PrefixType = PathPrefixType.Local;

            var request = new HttpRequestMock { PathBase = "/mylocalprefix" };

            optionsMock.Setup(x => x.Value).Returns(options);

            httpClientFactoryMock.Setup(x => x.Create(HttpClientType.HttpClient))
                .Returns(httpClientMock.Object);

            var subject = new ProxyServerMiddleware(httpClientFactoryMock.Object, requestDelegate, optionsMock.Object);

            // Action
            subject.Invoke(new HttpContextMock(request)).Wait();

            // Assets
            httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(p => p.RequestUri.LocalPath == "/mylocalprefix"), HttpCompletionOption.ResponseHeadersRead));
        }

        [Test]
        public void Invoke_WhenNoLocalPathIsDefined_ShouldUseTheDefaultLocalPath()
        {
            // Arrange
            var requestDelegate = fixture.Create<RequestDelegate>();
            var httpClientMock = fixture.Freeze<Mock<IProxyHttpClient>>();

            var options = CreateProxyOptions();

            optionsMock.Setup(x => x.Value).Returns(options);

            httpClientFactoryMock.Setup(x => x.Create(HttpClientType.HttpClient))
                .Returns(httpClientMock.Object);

            var subject = new ProxyServerMiddleware(httpClientFactoryMock.Object, requestDelegate, optionsMock.Object);

            // Action
            subject.Invoke(new HttpContextMock()).Wait();

            // Assets
            httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(p => p.RequestUri.LocalPath == "/"), HttpCompletionOption.ResponseHeadersRead));
        }

        [Test]
        public void Invoke_WhenAbsolutePathIsSetted_ShouldUseTheSettedAbsolutePath()
        {
            // Arrange
            var requestDelegate = fixture.Create<RequestDelegate>();
            var httpClientMock = fixture.Freeze<Mock<IProxyHttpClient>>();

            var options = CreateProxyOptions();

            var request = new HttpRequestMock { Path = "/myPath" };

            optionsMock.Setup(x => x.Value).Returns(options);

            httpClientFactoryMock.Setup(x => x.Create(HttpClientType.HttpClient))
                .Returns(httpClientMock.Object);

            var subject = new ProxyServerMiddleware(httpClientFactoryMock.Object, requestDelegate, optionsMock.Object);

            // Action
            subject.Invoke(new HttpContextMock(request)).Wait();

            // Assets
            httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(p => p.RequestUri.AbsolutePath == "/myPath"), HttpCompletionOption.ResponseHeadersRead));
        }

        [Test]
        public void Invoke_WhenAbsolutePathIsNotSetted_ShouldUseTheDefaultAbsolutePath()
        {
            // Arrange
            var requestDelegate = fixture.Create<RequestDelegate>();
            var httpClientMock = fixture.Freeze<Mock<IProxyHttpClient>>();

            var options = CreateProxyOptions();

            optionsMock.Setup(x => x.Value).Returns(options);

            httpClientFactoryMock.Setup(x => x.Create(HttpClientType.HttpClient))
                .Returns(httpClientMock.Object);

            var subject = new ProxyServerMiddleware(httpClientFactoryMock.Object, requestDelegate, optionsMock.Object);

            // Action
            subject.Invoke(new HttpContextMock()).Wait();

            // Assets
            httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(p => p.RequestUri.AbsolutePath == "/"), HttpCompletionOption.ResponseHeadersRead));
        }

        [Test]
        public void Invoke_WhenQueryStringIsSetted_ShouldUseTheSettedQuery()
        {
            // Arrange
            var requestDelegate = fixture.Create<RequestDelegate>();
            var httpClientMock = fixture.Freeze<Mock<IProxyHttpClient>>();

            var options = CreateProxyOptions();

            var request = new HttpRequestMock { QueryString = new QueryString("?myQueryString") };

            optionsMock.Setup(x => x.Value).Returns(options);

            httpClientFactoryMock.Setup(x => x.Create(HttpClientType.HttpClient))
                .Returns(httpClientMock.Object);

            var subject = new ProxyServerMiddleware(httpClientFactoryMock.Object, requestDelegate, optionsMock.Object);

            // Action
            subject.Invoke(new HttpContextMock(request)).Wait();

            // Assets
            httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(p => p.RequestUri.Query == "?myQueryString"), HttpCompletionOption.ResponseHeadersRead));
        }

        [Test]
        public void Invoke_WhenQueryStringIsNotSetted_ShouldNotUseAnyQuery()
        {
            // Arrange
            var requestDelegate = fixture.Create<RequestDelegate>();
            var httpClientMock = fixture.Freeze<Mock<IProxyHttpClient>>();

            var options = CreateProxyOptions();

            optionsMock.Setup(x => x.Value).Returns(options);

            httpClientFactoryMock.Setup(x => x.Create(HttpClientType.HttpClient))
                .Returns(httpClientMock.Object);

            var subject = new ProxyServerMiddleware(httpClientFactoryMock.Object, requestDelegate, optionsMock.Object);

            // Action
            subject.Invoke(new HttpContextMock()).Wait();

            // Assets
            httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(p => p.RequestUri.Query == ""), HttpCompletionOption.ResponseHeadersRead));
        }

        [Test]
        public void Invoke_WhenRequestHeaderDoesExist_ShouldUseThem()
        {
            // Arrange
            var requestDelegate = fixture.Create<RequestDelegate>();
            var httpClientMock = fixture.Freeze<Mock<IProxyHttpClient>>();
            var expectedHeaders = new HeaderDictionary {{"Cach-Control", "no-cache"}};

            var options = CreateProxyOptions();

            optionsMock.Setup(x => x.Value).Returns(options);

            httpClientFactoryMock.Setup(x => x.Create(HttpClientType.HttpClient))
                .Returns(httpClientMock.Object);

            var subject = new ProxyServerMiddleware(httpClientFactoryMock.Object, requestDelegate, optionsMock.Object);

            // Action
            subject.Invoke(new HttpContextMock(new HttpRequestMock(expectedHeaders))).Wait();

            // Assets
            httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(p => IsMatch(expectedHeaders, p.Headers)), HttpCompletionOption.ResponseHeadersRead));
        }

        [Test]
        public void Invoke_WhenRequestHeaderDoesNotExist_ShouldNotUseAnyHeaders()
        {
            // Arrange
            var requestDelegate = fixture.Create<RequestDelegate>();
            var httpClientMock = fixture.Freeze<Mock<IProxyHttpClient>>();

            var options = CreateProxyOptions();

            optionsMock.Setup(x => x.Value).Returns(options);

            httpClientFactoryMock.Setup(x => x.Create(HttpClientType.HttpClient))
                .Returns(httpClientMock.Object);

            var subject = new ProxyServerMiddleware(httpClientFactoryMock.Object, requestDelegate, optionsMock.Object);

            // Action
            subject.Invoke(new HttpContextMock(new HttpRequestMock())).Wait();

            // Assets
            httpClientMock.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(p => !p.Headers.ToList().Any()), HttpCompletionOption.ResponseHeadersRead));
        }

        [Test]
        public void Invoke_WhenResponseHeaderDoesExist_ShouldUseThem()
        {
            // Arrange
            var httpClientMock = fixture.Freeze<Mock<IProxyHttpClient>>();

            var options = CreateProxyOptions();

            var response = new HttpResponseMessage();
            response.Headers.Add("Cache-Control", "no-cache");

            optionsMock.Setup(x => x.Value).Returns(options);

            httpClientMock.Setup(x =>x.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead))
                .Returns(Task.FromResult(response));

            httpClientFactoryMock.Setup(x => x.Create(HttpClientType.HttpClient))
                .Returns(httpClientMock.Object);

            IHeaderDictionary result = null;
            var subject = new ProxyServerMiddleware(httpClientFactoryMock.Object, delegate(HttpContext context)
            {
                result = context.Response.Headers;
                return Task.FromResult(0);
            }, optionsMock.Object);

            // Action
            subject.Invoke(new HttpContextMock(new HttpRequestMock())).Wait();

            // Assets
            Assert.AreEqual(1, result.Count);
            Assert.IsNotNull(result["Cache-Control"]);
            Assert.AreEqual("no-cache", result["Cache-Control"]);
        }

        [Test]
        public void Invoke_WhenResponseContentHeaderDoesExist_ShouldUseThem()
        {
            // Arrange
            var httpClientMock = fixture.Freeze<Mock<IProxyHttpClient>>();

            var options = CreateProxyOptions();

            var response = new HttpResponseMessage { Content = new HttpContentMock() };
            response.Content.Headers.Add("Content-Type", "application/json");

            optionsMock.Setup(x => x.Value).Returns(options);

            httpClientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead))
                .Returns(Task.FromResult(response));

            httpClientFactoryMock.Setup(x => x.Create(HttpClientType.HttpClient))
                .Returns(httpClientMock.Object);

            IHeaderDictionary result = null;
            var subject = new ProxyServerMiddleware(httpClientFactoryMock.Object, delegate (HttpContext context)
            {
                result = context.Response.Headers;
                return Task.FromResult(0);
            }, optionsMock.Object);

            // Action
            subject.Invoke(new HttpContextMock(new HttpRequestMock(), new HttpResponseMock())).Wait();

            // Assets
            Assert.AreEqual(1, result.Count);
            Assert.IsNotNull(result["Content-Type"]);
            Assert.AreEqual("application/json", result["Content-Type"]);
        }

        [Test]
        public void Invoke_WhenResponseHeaderDoesNotExist_ShouldNotUseAnyHeaders()
        {
            // Arrange
            var httpClientMock = fixture.Freeze<Mock<IProxyHttpClient>>();

            var options = CreateProxyOptions();

            var response = new HttpResponseMessage();

            optionsMock.Setup(x => x.Value).Returns(options);

            httpClientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead))
                .Returns(Task.FromResult(response));

            httpClientFactoryMock.Setup(x => x.Create(HttpClientType.HttpClient))
                .Returns(httpClientMock.Object);

            IHeaderDictionary result = null;
            var subject = new ProxyServerMiddleware(httpClientFactoryMock.Object, delegate (HttpContext context)
            {
                result = context.Response.Headers;
                return Task.FromResult(0);
            }, optionsMock.Object);

            // Action
            subject.Invoke(new HttpContextMock(new HttpRequestMock())).Wait();

            // Assets
            Assert.AreEqual(0, result.Count);
        }

        private static ProxyOptions CreateProxyOptions()
        {
            return new ProxyOptions
            {
                Host = "localhost",
                Port = "80",
                Scheme = "http"
            };
        }

        private static bool IsMatch(IHeaderDictionary expected, HttpHeaders result)
        {
            if (expected.Count != result.ToList().Count)
            {
                return false;
            }

            foreach (var header in expected)
            {
                if (!result.Contains(header.Key))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
