#if(NET6_0_OR_GREATER)
using EasyMicroservice.Laboratory.Tests.Engine.Net;
using EasyMicroservices.Laboratory.Engine;
using EasyMicroservices.Laboratory.Engine.Net;
using EasyMicroservices.Laboratory.Engine.Net.Http;

namespace EasyMicroservices.Laboratory.Tests.Engine.Net
{
    public class HostHttpHandlerTest : BaseHandlerTest
    {
        protected override BaseHandler GetHandler(ResourceManager resourceManager)
        {
            return new HostHttpHandler(resourceManager);
        }
    }
}
#endif