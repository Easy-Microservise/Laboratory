using System;
using System.Collections.Concurrent;
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
            requestBody = NormalizeOSText(requestBody);
            resposneBody = NormalizeOSText(resposneBody);
            var spaceDetail = SpaceDetail.Load(requestBody, resposneBody);
            Spaces.TryAdd(resposneBody, spaceDetail);
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
            requestBody = NormalizeOSText(requestBody);
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
