using EasyMicroservices.Laboratory.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EasyMicroservices.Laboratory.Engine
{
    /// <summary>
    /// 
    /// </summary>
    public class ResourceManager
    {
        internal Scope CurrentScope { get; set; } = new Scope();
        internal HashSet<Scope> Scopes { get; set; } = new HashSet<Scope>();

        /// <summary>
        /// append request and response
        /// </summary>
        /// <param name="requestBody"></param>
        /// <param name="resposneBody"></param>
        public void Append(string requestBody, string resposneBody)
        {
            CurrentScope.Append(requestBody, resposneBody);
        }

        /// <summary>
        /// append request and response
        /// </summary>
        public void Append(Scope scope)
        {
            Scopes.Add(scope);
        }
    }
}
