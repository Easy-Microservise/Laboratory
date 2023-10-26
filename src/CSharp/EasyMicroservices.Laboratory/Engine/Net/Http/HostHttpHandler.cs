#if(NET6_0_OR_GREATER)
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EasyMicroservices.Laboratory.Engine.Net.Http
{
    /// <summary>
    /// 
    /// </summary>
    public class HostHttpHandler : HostHttpHandlerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceManager"></param>
        public HostHttpHandler(ResourceManager resourceManager) : base(resourceManager)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected override async Task<bool> HandleHttpClient(HttpContext httpClient)
        {
            var reader = new StreamReader(httpClient.Request.Body);
            var requestBody = await reader.ReadToEndAsync();
            var firstLine = $"{httpClient.Request.Method} {httpClient.Request.Path} {httpClient.Request.Protocol}";
            var headers = httpClient.Request.Headers.ToDictionary((x) => x.Key, (v) => v.Value.FirstOrDefault());
            StringBuilder fullBody = new StringBuilder();
            fullBody.AppendLine(firstLine);
            foreach (var item in headers.OrderBy(x => x.Key))
            {
                fullBody.AppendLine($"{item.Key}: {item.Value}");
            }
            fullBody.Append(requestBody);
            var responseBody = await WriteResponseAsync(firstLine, headers, requestBody, fullBody);
            using (var responseReader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(responseBody))))
            {
                await responseReader.ReadLineAsync();
                do
                {
                    var line = await responseReader.ReadLineAsync();
                    if (string.IsNullOrEmpty(line))
                        break;
                    var header = line.Split(':');
                    if (header[0].Equals("content-length", StringComparison.OrdinalIgnoreCase))
                        continue;
                    httpClient.Response.Headers.Add(header[0], header[1]);
                }
                while (true);
                var body = await responseReader.ReadToEndAsync();
                var bytes = Encoding.UTF8.GetBytes(body);
                httpClient.Response.ContentLength = bytes.Length;

                await httpClient.Response.Body.WriteAsync(bytes, 0, bytes.Length);
            }
            return true;
        }
    }
}
#endif