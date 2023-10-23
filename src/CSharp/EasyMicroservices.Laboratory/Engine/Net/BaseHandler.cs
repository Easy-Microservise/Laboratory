using EasyMicroservices.Laboratory.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
    }
}