using EasyMicroservices.Laboratory.Models;
using System.Collections.Concurrent;

namespace EasyMicroservices.Laboratory.Engine
{
    /// <summary>
    /// 
    /// </summary>
    public class ResourceManager
    {
        internal ConcurrentDictionary<string, SpaceDetail> Spaces { get; set; } = new ConcurrentDictionary<string, SpaceDetail>();

        /// <summary>
        /// append request and response
        /// </summary>
        /// <param name="requestBody"></param>
        /// <param name="resposneBody"></param>
        public void Append(string requestBody, string resposneBody)
        {
            var spaceDetail = SpaceDetail.Load(requestBody, resposneBody);
            Spaces.TryAdd(resposneBody, spaceDetail);
        }
    }
}
