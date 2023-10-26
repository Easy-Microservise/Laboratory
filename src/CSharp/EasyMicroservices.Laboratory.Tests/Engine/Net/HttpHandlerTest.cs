using EasyMicroservice.Laboratory.Tests.Engine.Net;
using EasyMicroservices.Laboratory.Engine;
using EasyMicroservices.Laboratory.Engine.Net;
using EasyMicroservices.Laboratory.Engine.Net.Http;

namespace EasyMicroservices.Laboratory.Tests.Engine.Net
{
    public class HttpHandlerTest : BaseHandlerTest
    {
        protected override BaseHandler GetHandler(ResourceManager resourceManager)
        {
            return new HttpHandler(resourceManager);
            //return BaseHandler.CreateOSHandler(resourceManager);
        }
    }
}