using EasyMicroservices.Laboratory.IO;
using EasyMicroservices.Utilities.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EasyMicroservices.Laboratory.Engine.Net
{
    /// <summary>
    /// Http protocol handler of Laboratory
    /// </summary>
    public class TcpHandler : TcpHandlerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceManager"></param>
        public TcpHandler(ResourceManager resourceManager) : base(resourceManager)
        {

        }


        /// <summary>
        /// Handle Tcp client of a http client
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <returns></returns>
        protected override async Task HandleTcpClient(TcpClient tcpClient)
        {
            using var stream = new PipelineStream(tcpClient.GetStream());
            string firstLine = await ReadLineAsync(stream);
            StringBuilder fullBody = new StringBuilder();
            Dictionary<string, string> headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            fullBody.AppendLine(firstLine);
            //read headers
            while (true)
            {
                var line = await ReadLineAsync(stream);
                if (string.IsNullOrEmpty(line))
                    break;
                var headerValue = line.Split(new char[] { ':' }, 2);
                headers.TryAddItem(headerValue[0], headerValue[1].Trim());
            }

            foreach (var item in headers.OrderBy(x => x.Key))
            {
                fullBody.AppendLine($"{item.Key}: {item.Value}");
            }

            int contentLength = 0;
            if (headers.TryGetValue(HttpHeadersConstants.ContentLength, out string contentLengthValue))
            {
                contentLength = int.Parse(contentLengthValue);
            }

            string requestBody = "";
            if (contentLength > 0)
            {
                var buffer = await ReadBlockAsync(stream, contentLength);
                requestBody = Encoding.UTF8.GetString(buffer);
                fullBody.AppendLine();
                fullBody.Append(requestBody);
            }
            await WriteResponseAsync(firstLine, headers, requestBody, fullBody, stream);
            tcpClient.Client.Shutdown(SocketShutdown.Send);
        }
    }
}
