using EasyMicroservices.Laboratory.Models;
using System.Linq;
using System.Threading.Tasks;

namespace EasyMicroservices.Laboratory.Engine
{
    /// <summary>
    /// Handle the requests
    /// </summary>
    public class RequestHandler
    {
        readonly ResourceManager _resourceManager;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceManager"></param>
        public RequestHandler(ResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }

        /// <summary>
        /// Find a response for a specified request
        /// </summary>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        public async Task<string> FindResponseBody(string requestBody)
        {
            var spaceDetail = FindSpace(requestBody);
            //var extractedSpace = SpaceDetail.GetSpaceDetail(requestBody, spaceDetail);
            return spaceDetail == null ? null : await spaceDetail.GetResponse();
        }

        SpaceDetail FindSpace(string requestBody)
        {
            var result = _resourceManager.CurrentScope.FindSpace(requestBody);
            if (result != null)
                return result;

            foreach (var scope in _resourceManager.Scopes)
            {
                result = scope.FindNext(requestBody);
                if (result != null)
                    return result;
            }
            return default;
        }
    }
}
