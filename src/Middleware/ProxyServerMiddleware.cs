using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ProxyServer.Factory;
using ProxyServer.Model;

namespace ProxyServer.Middleware
{
    public class ProxyServerMiddleware
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly RequestDelegate next;
        private readonly ProxyOptions options;

        public ProxyServerMiddleware(
            IHttpClientFactory httpClientFactory,
            RequestDelegate next, 
            IOptions<ProxyOptions> options)
        {
            this.httpClientFactory = httpClientFactory;
            this.next = next;
            this.options = options.Value;

            if (string.IsNullOrEmpty(this.options.Port))
            {
                this.options.Port = string.Equals(this.options.Scheme, "https", StringComparison.OrdinalIgnoreCase) ? "443" : "80";
            }

            if (string.IsNullOrEmpty(this.options.Scheme))
            {
                this.options.Scheme = "http";
            }

            if (string.IsNullOrEmpty(this.options.Host))
            {
                throw new ArgumentException("Host is undefined");
            }
        }

        public async Task Invoke(HttpContext context)
        {
            var httpClient = httpClientFactory.Create(HttpClientType.HttpClient);

            var requestMessage = CreateHttpRequestMessage(context);
            using (var responseMessage = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead))
            {
                await UpdateHttpResponseMessage(context, responseMessage);
            }

            await next(context);
        }

        private HttpRequestMessage CreateHttpRequestMessage(HttpContext context)
        {
            var requestMessage = new HttpRequestMessage();
            var requestMethod = context.Request.Method;

            if (!requestMethod.Contains("GET") &&
                !requestMethod.Contains("HEAD") &&
                !requestMethod.Contains("DELETE") &&
                !requestMethod.Contains("TRACE"))
            {
                var streamContent = new StreamContent(context.Request.Body);
                requestMessage.Content = streamContent;
            }

            CopyRequestHeaders(context.Request.Headers, requestMessage);

            var host = $"{options.Host}:{options.Port}";
            var pathBase = GetPathBase(context);
            var uriString = $"{options.Scheme}://{host}{pathBase}{context.Request.Path}{context.Request.QueryString}";

            requestMessage.RequestUri = new Uri(uriString);
            requestMessage.Method = new HttpMethod(context.Request.Method);
            return requestMessage;
        }

        private static async Task UpdateHttpResponseMessage(HttpContext context, HttpResponseMessage responseMessage)
        {
            context.Response.StatusCode = (int) responseMessage.StatusCode;

            CopyResponseHeaders(responseMessage.Headers, context);
            CopyResponseHeaders(responseMessage.Content?.Headers, context);

            context.Response.Headers.Remove("transfer-encoding");

            if (responseMessage.Content != null)
            {
                await responseMessage.Content.CopyToAsync(context.Response.Body);
            }
        }

        private static void CopyRequestHeaders(IHeaderDictionary headers, HttpRequestMessage request)
        {
            if (headers == null)
            {
                return;
            }

            foreach (var header in headers)
            {
                if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                {
                    request.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }
        }

        private static void CopyResponseHeaders(HttpHeaders headers, HttpContext context)
        {
            if (headers == null)
            {
                return;
            }

            foreach (var header in headers)
            {
                context.Response.Headers[header.Key] = header.Value.First();
            }
        }

        private string GetPathBase(HttpContext context)
        {
            switch (options.PrefixType)
            {
                case PathPrefixType.Local:
                    return context.Request.PathBase.ToString();
                case PathPrefixType.Defined:
                    return options.DefinedPrefix;
                default:
                    return string.Empty;
            }
        }
    }
}