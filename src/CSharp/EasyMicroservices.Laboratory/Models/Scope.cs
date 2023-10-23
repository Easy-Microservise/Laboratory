using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EasyMicroservices.Laboratory.Models
{
    /// <summary>
    /// scope of multiple request and responses
    /// </summary>
    public class Scope
    {
        internal ConcurrentDictionary<string, SpaceDetail> Spaces { get; set; } = new ConcurrentDictionary<string, SpaceDetail>();
        internal ConcurrentDictionary<string, SpaceDetail> Nexts { get; set; } = new ConcurrentDictionary<string, SpaceDetail>();

        internal int CurrentIndex { get; set; }

        /// <summary>
        /// append request and response
        /// </summary>
        /// <param name="requestBody"></param>
        /// <param name="resposneBody"></param>
        public void Append(string requestBody, string resposneBody)
        {
            requestBody = OrderHeaders(NormalizeOSText(requestBody));
            resposneBody = NormalizeOSText(resposneBody);
            var spaceDetail = SpaceDetail.Load(requestBody, resposneBody);
            Spaces.TryAdd(resposneBody, spaceDetail);
        }

        string OrderHeaders(string request)
        {
            string firstLine = "";
            using (var reader = new StreamReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(request))))
            {
                firstLine = reader.ReadLine();
                if (!firstLine.ToLower().Contains("http/"))
                    return request;
                Dictionary<string, string> headers = new Dictionary<string, string>();
                do
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        break;
                    var header = line.Split(':');
                    headers.Add(header[0], header[1]);

                }
                while (true);
                var builder = new System.Text.StringBuilder();
                builder.AppendLine(firstLine);
                foreach (var header in headers.OrderBy(x=>x.Key))
                {
                    builder.AppendLine($"{header.Key}:{header.Value}");
                }
                builder.Append(reader.ReadToEnd());
                return builder.ToString();
            }
        }

        int GetMax()
        {
            return Nexts.Values.DefaultIfEmpty(new SpaceDetail()).Max(x => x.Index);
        }

        /// <summary>
        /// append request and response
        /// </summary>
        /// <param name="requestBody"></param>
        /// <param name="resposneBody"></param>
        public void AppendNext(string requestBody, string resposneBody)
        {
            requestBody = OrderHeaders(NormalizeOSText(requestBody));
            resposneBody = NormalizeOSText(resposneBody);
            var spaceDetail = SpaceDetail.Load(requestBody, resposneBody);
            spaceDetail.Index = GetMax();
            spaceDetail.Index++;
            Nexts.TryAdd(resposneBody, spaceDetail);
        }

        internal SpaceDetail FindSpace(string requestBody)
        {
            requestBody = NormalizeOSText(requestBody);
            foreach (var space in Spaces.Values.ToArray())
            {
                if (space.IsValid(requestBody))
                    return space;
            }
            return default;
        }

        internal SpaceDetail FindNext(string requestBody)
        {
            requestBody = NormalizeOSText(requestBody);
            foreach (var space in Nexts.Values.OrderBy(x => x.Index).ToArray())
            {
                if (space.IsValid(requestBody) && space.Index > CurrentIndex)
                {
                    CurrentIndex++;
                    if (CurrentIndex >= GetMax())
                        CurrentIndex = 0;
                    return space;
                }
            }
            return default;
        }

        string NormalizeOSText(string text)
        {
            if (System.Environment.OSVersion.Platform == PlatformID.Unix)
                return text.Replace("\r\n", "\n");
            return text;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            Spaces.Clear();
            Nexts.Clear();
        }
    }
}
