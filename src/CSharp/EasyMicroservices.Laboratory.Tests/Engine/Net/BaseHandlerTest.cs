using EasyMicroservices.Laboratory.Constants;
using EasyMicroservices.Laboratory.Engine;
using EasyMicroservices.Laboratory.Engine.Net;
using EasyMicroservices.Laboratory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace EasyMicroservice.Laboratory.Tests.Engine.Net
{
    public abstract class BaseHandlerTest
    {
        public HttpClient GetHttpClient()
        {
            HttpClient httpClient = default;
//            if (System.Environment.OSVersion.Platform != PlatformID.Unix)
//            {
//#if (NET452)
//                httpClient = new HttpClient();
//#else
//                var handler = new WinHttpHandler();
//                httpClient = new HttpClient(handler);
//#endif
//            }
//            else
                httpClient = new HttpClient();

#if (!NET452 && !NET48)
            httpClient.DefaultRequestVersion = HttpVersion.Version20;
#endif
            return httpClient;
        }
        protected abstract BaseHandler GetHandler(ResourceManager resourceManager);
        public string GetHttpResponseHeaders(string response)
        {
            return @$"HTTP/1.1 200 OK
Cache-Control: no-cache
Pragma: no-cache
Content-Type: application/json; charset=utf-8
Vary: Accept-Encoding
Date: Mon, 16 Mar 2020 07:48:17 GMT

{response}";
        }

        string NormalizeOSText(string text)
        {
#if (!NET452 && !NET48)
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                return text.Replace("\r\n", "\n").Replace("Content-Length: 21", "Content-Length: 20");
#endif
            return text;
        }
        [Theory]
        [InlineData($"Hello Ali \r\n Hi Mahdi", "Reza")]
        [InlineData("Hello Ali", "Reza \n")]
        [InlineData("Hello Mahdi", "Body \r\n Body2")]
        public async Task CheckSimpleRequestAndResponse(string request, string response)
        {
            request = NormalizeOSText(request);
            response = NormalizeOSText(response);
            ResourceManager resourceManager = new ResourceManager();
            resourceManager.Append(request, GetHttpResponseHeaders(response));
            var port = await GetHandler(resourceManager).Start();

            HttpClient httpClient = GetHttpClient();
            var data = new StringContent(request);
            var httpResponse = await httpClient.PostAsync($"http://localhost:{port}", data);
            Assert.Equal(await httpResponse.Content.ReadAsStringAsync(), response);
        }

        [Theory]
        [InlineData($"Hello Ali \r\n Hi Mahdi", "Reza")]
        [InlineData($"Hello Ali \r\n Hi Mahdi2", "Reza2")]
        [InlineData($"Hello Ali \r\n Hi Mahdi3", "Reza3")]
        public async Task ConcurrentCheckSimpleRequestAndResponse(string request, string response)
        {
            request = NormalizeOSText(request);
            response = NormalizeOSText(response);
            ResourceManager resourceManager = new ResourceManager();
            resourceManager.Append(request, GetHttpResponseHeaders(response));
            var port = await GetHandler(resourceManager).Start();

            List<Task<bool>> all = new List<Task<bool>>();
            for (int i = 0; i < 100; i++)
            {
                all.Add(Task.Run(async () =>
                {
                    HttpClient httpClient = GetHttpClient();
                    var data = new StringContent(request);
                    var httpResponse = await httpClient.PostAsync($"http://localhost:{port}", data);
                    Assert.Equal(await httpResponse.Content.ReadAsStringAsync(), response);
                    return true;
                }));
            }
            await Task.WhenAll(all);
            Assert.True(all.All(x => x.Result));
        }

        [Theory]
        [InlineData($"Hello Ali \r\n Hi Mahdi", "Reza")]
        [InlineData($"Hello Ali \r\n Hi Mahdi2", "Reza2")]
        [InlineData($"Hello Ali \r\n Hi Mahdi3", "Reza3")]
        public async Task ConcurrentSingleHttpClientCheckSimpleRequestAndResponse(string request, string response)
        {
            request = NormalizeOSText(request);
            response = NormalizeOSText(response);
            ResourceManager resourceManager = new ResourceManager();
            resourceManager.Append(request, GetHttpResponseHeaders(response));
            var port = await GetHandler(resourceManager).Start();
            HttpClient httpClient = GetHttpClient();

            List<Task<bool>> all = new List<Task<bool>>();
            for (int i = 0; i < 5; i++)
            {
                all.Add(Task.Run(async () =>
                {
                    var data = new StringContent(request);
                    var httpResponse = await httpClient.PostAsync($"http://localhost:{port}", data);
                    Assert.Equal(await httpResponse.Content.ReadAsStringAsync(), response);
                    return true;
                }));
            }
            await Task.WhenAll(all);
            Assert.True(all.All(x => x.Result));
        }

        [Theory]
        [InlineData("Hello Ali \r\n Hi Mahdi", $"POST / HTTP/1.1\r\nContent-Length: 21\r\nContent-Type: text/plain; charset=utf-8\r\nHost: localhost:*MyPort*\r\n\r\nHello Ali \r\n Hi Mahdi")]
        public async Task CheckSimpleRequestToGiveMeFullRequestHeaderValue(string request, string response)
        {
            request = NormalizeOSText(request);
            response = NormalizeOSText(response);
            ResourceManager resourceManager = new ResourceManager();
            var port = await GetHandler(resourceManager).Start();
            response = response.Replace("*MyPort*", port.ToString());
            HttpClient httpClient = GetHttpClient();
            httpClient.DefaultRequestHeaders.ExpectContinue = false;
            httpClient.DefaultRequestHeaders.Add(RequestTypeHeaderConstants.RequestTypeHeader, RequestTypeHeaderConstants.GiveMeFullRequestHeaderValue);
            var data = new StringContent(request);
            var httpResponse = await httpClient.PostAsync($"http://localhost:{port}", data);
            var textResponse = await httpResponse.Content.ReadAsStringAsync();
#if (!NET452 && !NET48)
            Assert.Equal(textResponse, response);
#endif
        }

        [Theory]
        [InlineData("Hello Ali \r\n Hi Mahdi", $"POST / HTTP/1.1\r\nContent-Length: 21\r\nContent-Type: text/plain; charset=utf-8\r\nHost: localhost:*MyPort*\r\n\r\nHello Ali \r\n Hi Mahdi")]
        public async Task CheckSimpleRequestToGiveMeLastFullRequestHeaderValue(string request, string response)
        {
            request = NormalizeOSText(request);
            response = NormalizeOSText(response);
            ResourceManager resourceManager = new ResourceManager();
            var port = await GetHandler(resourceManager).Start();
            response = response.Replace("*MyPort*", port.ToString());
            HttpClient httpClient = GetHttpClient();
            httpClient.DefaultRequestHeaders.ExpectContinue = false;
            httpClient.DefaultRequestHeaders.Add(RequestTypeHeaderConstants.RequestTypeHeader, RequestTypeHeaderConstants.GiveMeFullRequestHeaderValue);
            var data = new StringContent(request);
            var httpResponse = await httpClient.PostAsync($"http://localhost:{port}", data);
            var textResponse = await httpResponse.Content.ReadAsStringAsync();

            httpClient = GetHttpClient();
            httpClient.DefaultRequestHeaders.Add(RequestTypeHeaderConstants.RequestTypeHeader, RequestTypeHeaderConstants.GiveMeLastFullRequestHeaderValue);
            httpResponse = await httpClient.GetAsync($"http://localhost:{port}");
            textResponse = await httpResponse.Content.ReadAsStringAsync();
#if (!NET452 && !NET48)
            Assert.Equal(textResponse, response);
#endif
        }

        [Theory]
        [InlineData(@"PUT*RequestSkipBody*HTTP/1.1
x-amz-meta-title: *RequestSkipBody*
User-Agent: *RequestSkipBody*
amz-sdk-invocation-id: *RequestSkipBody*
amz-sdk-request: *RequestSkipBody*
X-Amz-Date: *RequestSkipBody*
X-Amz-Content-SHA256: *RequestSkipBody*
Authorization: *RequestSkipBody*
Host: *RequestSkipBody*
Content-Length: *RequestSkipBody*"
,
@"HTTP/1.1 200 OK
x-amz-id-2: Ali/Reza/Javad
x-amz-request-id: id
Date: Sat, 07 Jan 2023 16:24:56 GMT
x-amz-version-id: AmazonVersion
ETag: ""ETag""
Server: AmazonS3
Content-Length: 0

Ali", "Ali")]
        public async Task CheckComplex(string request, string response, string simpleResponse)
        {
            request = NormalizeOSText(request);
            response = NormalizeOSText(response);
            ResourceManager resourceManager = new ResourceManager();
            resourceManager.Append(request, response);
            var port = await GetHandler(resourceManager).Start();
            HttpClient httpClient = GetHttpClient();
            httpClient.DefaultRequestHeaders.Add("x-amz-meta-title", "someTitle");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "aws-sdk-dotnet-coreclr/3.7.101.44 aws-sdk-dotnet-core/3.7.103.6 .NET_Core/6.0.11 OS/Microsoft_Windows_10.0.22000 ClientAsync");
            httpClient.DefaultRequestHeaders.Add("amz-sdk-invocation-id", "guid");
            httpClient.DefaultRequestHeaders.Add("amz-sdk-request", "attempt=1; max=5");
            httpClient.DefaultRequestHeaders.Add("X-Amz-Date", "20230107T162454Z");
            httpClient.DefaultRequestHeaders.Add("X-Amz-Content-SHA256", "sha256");
            httpClient.DefaultRequestHeaders.Add("Authorization", "empty");
            httpClient.DefaultRequestHeaders.Add("Host", "s3.eu-west-1.amazonaws.com");
            var httpResponse = await httpClient.PutAsync($"http://localhost:{port}", null);
            var textResponse = await httpResponse.Content.ReadAsStringAsync();
            Assert.Equal(textResponse, simpleResponse);
        }

        [Fact]
        public async Task CheckScope()
        {
            ResourceManager resourceManager = new ResourceManager();
            var scope = new Scope();
            scope.AppendNext(NormalizeOSText(@"PUT*RequestSkipBody*HTTP/1.1
x-amz-meta-title: *RequestSkipBody*
User-Agent: *RequestSkipBody*
amz-sdk-invocation-id: *RequestSkipBody*
amz-sdk-request: *RequestSkipBody*
X-Amz-Date: *RequestSkipBody*
X-Amz-Content-SHA256: *RequestSkipBody*
Authorization: *RequestSkipBody*
Host: *RequestSkipBody*
Content-Length: *RequestSkipBody*")
,
NormalizeOSText(@"HTTP/1.1 200 OK
x-amz-id-2: Ali/Reza/Javad
x-amz-request-id: id
Date: Sat, 07 Jan 2023 16:24:56 GMT
x-amz-version-id: AmazonVersion
ETag: ""ETag""
Server: AmazonS3
Content-Length: 0

Ali"));
            scope.AppendNext(NormalizeOSText(@"PUT*RequestSkipBody*HTTP/1.1
x-amz-meta-title: *RequestSkipBody*
User-Agent: *RequestSkipBody*
amz-sdk-invocation-id: *RequestSkipBody*
amz-sdk-request: *RequestSkipBody*
X-Amz-Date: *RequestSkipBody*
X-Amz-Content-SHA256: *RequestSkipBody*
Authorization: *RequestSkipBody*
Host: *RequestSkipBody*
Content-Length: *RequestSkipBody*")
,
NormalizeOSText(@"HTTP/1.1 200 OK
x-amz-id-2: Ali/Reza/Javad
x-amz-request-id: id
Date: Sat, 07 Jan 2023 16:24:56 GMT
x-amz-version-id: AmazonVersion
ETag: ""ETag""
Server: AmazonS3
Content-Length: 0

Reza"));
            resourceManager.Append(scope);
            var port = await GetHandler(resourceManager).Start();
            var addHeaders = (HttpClient client) =>
            {
                client.DefaultRequestHeaders.Add("x-amz-meta-title", "someTitle");
                client.DefaultRequestHeaders.Add("User-Agent", "aws-sdk-dotnet-coreclr/3.7.101.44 aws-sdk-dotnet-core/3.7.103.6 .NET_Core/6.0.11 OS/Microsoft_Windows_10.0.22000 ClientAsync");
                client.DefaultRequestHeaders.Add("amz-sdk-invocation-id", "guid");
                client.DefaultRequestHeaders.Add("amz-sdk-request", "attempt=1; max=5");
                client.DefaultRequestHeaders.Add("X-Amz-Date", "20230107T162454Z");
                client.DefaultRequestHeaders.Add("X-Amz-Content-SHA256", "sha256");
                client.DefaultRequestHeaders.Add("Authorization", "empty");
                client.DefaultRequestHeaders.Add("Host", "s3.eu-west-1.amazonaws.com");
            };
            HttpClient httpClient = GetHttpClient();
            addHeaders(httpClient);
            var httpResponse = await httpClient.PutAsync($"http://localhost:{port}", null);
            var textResponse = await httpResponse.Content.ReadAsStringAsync();
            Assert.Equal("Ali", textResponse);

            httpClient = GetHttpClient();
            addHeaders(httpClient);
            httpResponse = await httpClient.PutAsync($"http://localhost:{port}", null);
            textResponse = await httpResponse.Content.ReadAsStringAsync();
            Assert.Equal("Reza", textResponse);

            httpClient = GetHttpClient();
            addHeaders(httpClient);
            httpResponse = await httpClient.PutAsync($"http://localhost:{port}", null);
            textResponse = await httpResponse.Content.ReadAsStringAsync();
            Assert.Equal("Ali", textResponse);
        }
    }
}