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
    public class HttpHandler : HttpHandlerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceManager"></param>
        public HttpHandler(ResourceManager resourceManager) : base(resourceManager)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected override async Task HandleHttpClient(HttpListenerContext httpClient)
        {
            var reader = new StreamReader(httpClient.Request.InputStream);
            var requestBody = await reader.ReadToEndAsync();
            var firstLine = $"{httpClient.Request.HttpMethod} {httpClient.Request.RawUrl} HTTP/{httpClient.Request.ProtocolVersion}";
            var headers = httpClient.Request.Headers.AllKeys.Select(x => new { Key = x, Value = httpClient.Request.Headers[x] }).ToDictionary((x) => x.Key, (v) => v.Value);
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
                    httpClient.Response.AddHeader(header[0], header[1]);
                }
                while (true);
                var body = await responseReader.ReadToEndAsync();
                var bytes = Encoding.UTF8.GetBytes(body);
                httpClient.Response.ContentLength64 = bytes.Length;

                await httpClient.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
            }
        }
    }
}
