#if (NET6_0_OR_GREATER)
using EasyMicroservices.Laboratory.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyMicroservices.Laboratory.Engine.Net.Http
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class HostHttpHandlerBase : BaseHandler
    {
        /// <summary>
        /// 
        /// </summary>
        public HostHttpHandlerBase(ResourceManager resourceManager) : base(resourceManager)
        {
        }

        /// <summary>
        /// Start the Http listener
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public override Task Start(int port)
        {
            return InternalStart(port);
        }

        /// <summary>
        /// start on any random port
        /// </summary>
        /// <returns>port to listen</returns>
        public override async Task<int> Start()
        {
            int port;
            while (true)
            {
                port = GetRandomPort();
                try
                {
                    await InternalStart(port);
                    break;
                }
                catch
                {
                }
            }
            return port;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Stop()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        protected virtual async Task InternalStart(int port)
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls($"http://*:{port}");
            var app = builder.Build();
            app.Use(async (context, next) =>
            {
                await HandleHttpClient(context);
                await next(context);
            });
            await Task.WhenAny(app.RunAsync(null), Task.Delay(3000));
        }

        /// <summary>
        /// Handle a Http client
        /// </summary>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        protected abstract Task HandleHttpClient(HttpContext httpClient);

        string _lastResponseBody = "";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstLine"></param>
        /// <param name="requestHeaders"></param>
        /// <param name="requestBody"></param>
        /// <param name="fullBody"></param>
        /// <returns></returns>
        public async Task<string> WriteResponseAsync(string firstLine, Dictionary<string, string> requestHeaders, string requestBody, StringBuilder fullBody)
        {
            string responseBody = "";
            if (requestHeaders.TryGetValue(RequestTypeHeaderConstants.RequestTypeHeader, out string headerTypeValue))
            {
                switch (headerTypeValue)
                {
                    case RequestTypeHeaderConstants.GiveMeFullRequestHeaderValue:
                        {
                            responseBody = GetGiveMeFullRequestHeaderValueResponse(firstLine, requestHeaders, requestBody);
                            break;
                        }
                    case RequestTypeHeaderConstants.GiveMeLastFullRequestHeaderValue:
                        {
                            responseBody = _lastResponseBody;
                            break;
                        }
                }
            }
            else
                responseBody = await _requestHandler.FindResponseBody(fullBody.ToString());
            if (string.IsNullOrEmpty(responseBody))
                responseBody = GetNoResponse(firstLine, requestHeaders, requestBody);
            _lastResponseBody = responseBody;
            return responseBody;
        }

        string GetGiveMeFullRequestHeaderValueResponse(string firstLine, Dictionary<string, string> requestHeaders, string requestBody)
        {
            StringBuilder responseBuilder = new();
            StringBuilder bodyBuilder = new();
            responseBuilder.AppendLine(DefaultResponse());
            bodyBuilder.AppendLine(firstLine);
            foreach (var header in requestHeaders.Where(x => !x.Key.Equals(RequestTypeHeaderConstants.RequestTypeHeader, StringComparison.OrdinalIgnoreCase)).OrderBy(x => x.Key))
            {
                bodyBuilder.Append(header.Key);
                bodyBuilder.Append(": ");
                bodyBuilder.AppendLine(header.Value);
            }
            bodyBuilder.AppendLine();
            bodyBuilder.Append(requestBody);

            responseBuilder.AppendLine($"Content-Length: {bodyBuilder.Length}");
            responseBuilder.AppendLine();
            responseBuilder.Append(bodyBuilder);

            return responseBuilder.ToString();
        }

        string DefaultResponse()
        {
            return @$"HTTP/1.1 200 OK
Cache-Control: no-cache
Pragma: no-cache
Content-Type: text/plain; charset=utf-8
Vary: Accept-Encoding";
        }

        string GetNoResponse(string firstLine, Dictionary<string, string> requestHeaders, string requestBody)
        {
            StringBuilder stringBuilder = new StringBuilder();
            StringBuilder bodyBuilder = new();
            bodyBuilder.AppendLine(firstLine);
            foreach (var header in requestHeaders.OrderBy(x => x.Key))
            {
                bodyBuilder.Append(header.Key);
                bodyBuilder.Append(": ");
                bodyBuilder.AppendLine(header.Value);
            }
            bodyBuilder.AppendLine();
            bodyBuilder.Append(requestBody);
            var defaultResponse = @$"HTTP/1.1 405 OK
Cache-Control: no-cache
Pragma: no-cache
Content-Type: text/plain; charset=utf-8
Vary: Accept-Encoding";
            stringBuilder.AppendLine(defaultResponse);
            stringBuilder.AppendLine($"Content-Length: {bodyBuilder.Length}");
            stringBuilder.AppendLine();
            stringBuilder.Append(bodyBuilder);
            return stringBuilder.ToString();
        }

        string MergeRequest(string firstLine, Dictionary<string, string> requestHeaders, string requestBody)
        {
            StringBuilder bodyBuilder = new();
            bodyBuilder.AppendLine(firstLine);
            foreach (var header in requestHeaders)
            {
                bodyBuilder.Append(header.Key);
                bodyBuilder.Append(": ");
                bodyBuilder.AppendLine(header.Value);
            }
            bodyBuilder.AppendLine();
            bodyBuilder.Append(requestBody);
            return bodyBuilder.ToString();
        }
    }
}
#endif