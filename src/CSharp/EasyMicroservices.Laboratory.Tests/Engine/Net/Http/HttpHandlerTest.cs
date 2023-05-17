using EasyMicroservices.Laboratory.Constants;
using EasyMicroservices.Laboratory.Engine;
using EasyMicroservices.Laboratory.Engine.Net.Http;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace EasyMicroservice.Laboratory.Tests.Engine.Net.Http
{
    public class HttpHandlerTest : BaseHandler
    {
        string NormalizeOSText(string text)
        {
#if (!NET452 && !NET48)
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                return text.Replace("\r\n", "\n");
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
            HttpHandler httpHandler = new HttpHandler(resourceManager);
            var port = await httpHandler.Start();

            HttpClient httpClient = new HttpClient();
            var data = new StringContent(request);
            var httpResponse = await httpClient.PostAsync($"http://localhost:{port}", data);
            Assert.Equal(await httpResponse.Content.ReadAsStringAsync(), response);
        }

        [Theory]
        [InlineData("Hello Ali \r\n Hi Mahdi", $"POST / HTTP/1.1\r\nHost: localhost:*MyPort*\r\nContent-Type: text/plain; charset=utf-8\r\nContent-Length: 21\r\n\r\nHello Ali \r\n Hi Mahdi")]
        public async Task CheckSimpleRequestToGiveMeFullRequestHeaderValue(string request, string response)
        {
            request = NormalizeOSText(request);
            response = NormalizeOSText(response);
            ResourceManager resourceManager = new ResourceManager();
            HttpHandler httpHandler = new HttpHandler(resourceManager);
            var port = await httpHandler.Start();
            response = response.Replace("*MyPort*", port.ToString());
            HttpClient httpClient = new HttpClient();
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
        [InlineData("Hello Ali \r\n Hi Mahdi", $"POST / HTTP/1.1\r\nHost: localhost:*MyPort*\r\nContent-Type: text/plain; charset=utf-8\r\nContent-Length: 21\r\n\r\nHello Ali \r\n Hi Mahdi")]
        public async Task CheckSimpleRequestToGiveMeLastFullRequestHeaderValue(string request, string response)
        {
            request = NormalizeOSText(request);
            response = NormalizeOSText(response);
            ResourceManager resourceManager = new ResourceManager();
            HttpHandler httpHandler = new HttpHandler(resourceManager);
            var port = await httpHandler.Start();
            response = response.Replace("*MyPort*", port.ToString());
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.ExpectContinue = false;
            httpClient.DefaultRequestHeaders.Add(RequestTypeHeaderConstants.RequestTypeHeader, RequestTypeHeaderConstants.GiveMeFullRequestHeaderValue);
            var data = new StringContent(request);
            var httpResponse = await httpClient.PostAsync($"http://localhost:{port}", data);
            var textResponse = await httpResponse.Content.ReadAsStringAsync();

            httpClient.DefaultRequestHeaders.Clear();
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
            HttpHandler httpHandler = new HttpHandler(resourceManager);
            var port = await httpHandler.Start();
            HttpClient httpClient = new HttpClient();
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
    }
}
