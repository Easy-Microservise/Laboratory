using EasyMicroservice.Laboratory.Constants;
using EasyMicroservice.Laboratory.Models.Spaces;
using EasyMicroservices.Utilities.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMicroservice.Laboratory.Models
{
    /// <summary>
    /// details of space
    /// </summary>
    public class SpaceDetail
    {
        /// <summary>
        /// unique hash of request
        /// </summary>
        public string Hash { get; set; }
        /// <summary>
        /// spaces
        /// </summary>
        public List<NormalTextSpace> RequestSpaces { get; set; } = new List<NormalTextSpace>();
        /// <summary>
        /// 
        /// </summary>
        public List<NormalTextSpace> ResponseSpaces { get; set; } = new List<NormalTextSpace>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool IsValid(string request)
        {
            int startFrom = 0;
            foreach (var space in RequestSpaces)
            {
                if (space is NormalTextSpace)
                {
                    var index = request.IndexOf(space.Text, startFrom);
                    if (index < 0)
                        return false;
                    startFrom = index + space.Text.Length;
                }
            }
            return true;
        }

        /// <summary>
        /// Load request template by requestbody
        /// </summary>
        /// <param name="requestBody"></param>
        /// <param name="responseBody"></param>
        public static SpaceDetail Load(string requestBody, string responseBody)
        {
            SpaceDetail result = new SpaceDetail();
            result.RequestSpaces.AddRange(LoadRequests(requestBody));
            result.ResponseSpaces.AddRange(LoadResponses(responseBody));
            result.Hash = HashHelper.GetSHA1Hash(result.RequestSpaces.Select(x => x.Text).ToArray());
            return result;
        }

        static IEnumerable<NormalTextSpace> LoadRequests(string requestBody)
        {
            var skipBodies = requestBody.Split(RequestConstants.SkipBody);
            if (skipBodies.Length > 1)
            {
                for (int i = 0; i < skipBodies.Length; i++)
                {
                    if (skipBodies[i] == "")
                        yield return new SkipBodySpace();
                    else
                        yield return new NormalTextSpace() { Text = skipBodies[i] };
                }
            }
            else
                yield return new NormalTextSpace() { Text = skipBodies[0] };
        }

        static IEnumerable<NormalTextSpace> LoadResponses(string requestBody)
        {
            yield return new NormalTextSpace() { Text = requestBody };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestBody"></param>
        /// <param name="spaceDetail"></param>
        /// <returns></returns>
        public static SpaceDetail GetSpaceDetail(string requestBody, SpaceDetail spaceDetail)
        {
            SpaceDetail result = new SpaceDetail();
            int startIndex = 0;
            foreach (var space in spaceDetail.RequestSpaces)
            {
                int length = space.Text.Length;
                var indexOf = requestBody.IndexOf(space.Text);
                result.RequestSpaces.Add(new NormalTextSpace()
                {
                    Text = requestBody.Substring(indexOf, length)
                });
                if (indexOf > startIndex)
                {
                    var skippedData = requestBody.Substring(startIndex, indexOf);
                    result.RequestSpaces.Add(new SkipBodySpace()
                    {
                        Text = skippedData
                    });
                }
                startIndex += indexOf + length;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetResponse()
        {
            StringBuilder result = new StringBuilder();
            foreach (var space in ResponseSpaces)
            {
                result.Append(space.Text);
            }
            return await GetHttpResponseBody(result.ToString());
        }

        async Task<string> GetHttpResponseBody(string response)
        {
            StringBuilder stringBuilder = new StringBuilder();
            using var memory = new MemoryStream(Encoding.UTF8.GetBytes(response));
            using var reader = new StreamReader(memory, Encoding.UTF8);
            string line;
            do
            {
                line = await reader.ReadLineAsync();
                if (line == "")
                {
                    var body = await reader.ReadToEndAsync();
                    stringBuilder.AppendLine($"Content-Length: {body.Length}");
                    stringBuilder.AppendLine("");
                    stringBuilder.Append(body);
                    break;
                }
                else if (line != null)
                    stringBuilder.AppendLine(line);
            }
            while (line != null);
            return stringBuilder.ToString();
        }
    }
}
