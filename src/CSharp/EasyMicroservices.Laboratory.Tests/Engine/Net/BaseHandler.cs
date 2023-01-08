using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMicroservice.Laboratory.Tests.Engine.Net
{
    public abstract class BaseHandler
    {
        public string GetHttpResponseHeaders(string response)
        {
            return @$"HTTP/1.1 200 OK
Cache-Control: no-cache
Pragma: no-cache
Content-Type: application/json; charset=utf-8
Vary: Accept-Encoding
Date: Mon, 16 Mar 2020 07:48:17 GMT

{response}";
        }
    }
}
