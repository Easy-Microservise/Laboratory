using EasyMicroservices.Laboratory.Engine.Net.Http;
using System;
using System.Threading.Tasks;

namespace EasyMicroservices.Laboratory.Engine.Net
{
    /// <summary>
    /// Handle the Tcp services
    /// </summary>
    public abstract class BaseHandler
    {
        /// <summary>
        /// 
        /// </summary>
        public BaseHandler(ResourceManager resourceManager)
        {
            _requestHandler = new RequestHandler(resourceManager);
        }

        /// <summary>
        /// 
        /// </summary>
        protected readonly RequestHandler _requestHandler;
        /// <summary>
        /// Start the Tcp listener
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public abstract Task Start(int port);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract Task<int> Start();

        /// <summary>
        /// 
        /// </summary>
        public abstract void Stop();


        static Random _random = new Random();
        /// <summary>
        /// start on any random port
        /// </summary>
        /// <returns>port to listen</returns>
        public int GetRandomPort()
        {
            return _random.Next(1111, 9999);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceManager"></param>
        /// <returns></returns>
        public static BaseHandler CreateOSHandler(ResourceManager resourceManager)
        {
#if (NET6_0_OR_GREATER)
            if (System.Environment.OSVersion.Platform == PlatformID.Unix)
                return new HostHttpHandler(resourceManager);
            else
                return new HttpHandler(resourceManager);
#else
            throw new NotSupportedException("Only support on net6.0 or grater!");
#endif
        }
    }
}