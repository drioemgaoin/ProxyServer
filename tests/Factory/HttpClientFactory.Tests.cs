using System;
using NUnit.Framework;
using ProxyServer.Factory;
using ProxyServer.Http;

namespace ProxyServer.Tests.Factory
{
    [TestFixture]
    public class HttpClientFactoryTests
    {
        [Test]
        public void Create_WhenRequestingAHttpClientInstance_ShouldCreateInstanceOfProxyHttpClient()
        {
            // Arrange
            var subject = new HttpClientFactory();

            // Action
            var result = subject.Create(Model.HttpClientType.HttpClient);

            // Assets
            Assert.IsInstanceOf<ProxyHttpClient>(result);
        }

        [Test]
        public void Create_WhenRequestingAnUnmanagedType_ShouldThrowException()
        {
            // Arrange
            var subject = new HttpClientFactory();

            // Action
            TestDelegate action = () => subject.Create(Model.HttpClientType.None);

            // Assets
            var exception = Assert.Throws<Exception>(action);
            Assert.AreEqual("None is an unknown http client type", exception.Message);
        }
    }
}
