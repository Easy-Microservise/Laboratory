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
        [Theory]
        [InlineData("Hello Ali \r\n Hi Mahdi", "Reza")]
        [InlineData("Hello Ali", "Reza \n")]
        [InlineData("Hello Mahdi", "Body \r\n Body2")]
        public async Task CheckSimpleRequestAndResponse(string request, string response)
        {
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
        [InlineData("Hello Ali \r\n Hi Mahdi", "POST / HTTP/1.1\r\nHost: localhost:2042\r\nContent-Type: text/plain; charset=utf-8\r\nContent-Length: 21\r\n\r\nHello Ali \r\n Hi Mahdi")]
        public async Task CheckSimpleRequestToGiveMeFullRequestHeaderValue(string request, string response)
        {
            ResourceManager resourceManager = new ResourceManager();
            resourceManager.Append(request, GetHttpResponseHeaders(response));
            HttpHandler httpHandler = new HttpHandler(resourceManager);
            var port = await httpHandler.Start();

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add(RequestTypeHeaderConstants.RequestTypeHeader, RequestTypeHeaderConstants.GiveMeFullRequestHeaderValue);
            var data = new StringContent(request);
            var httpResponse = await httpClient.PostAsync($"http://localhost:{port}", data);
            var textResponse = await httpResponse.Content.ReadAsStringAsync();
            Assert.Equal(textResponse, response);
        }
    }
}
