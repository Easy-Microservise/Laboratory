namespace EasyMicroservices.Laboratory.Constants
{
    /// <summary>
    /// 
    /// </summary>
    public class RequestTypeHeaderConstants
    {
        /// <summary>
        /// type of request header name
        /// </summary>
        public const string RequestTypeHeader = "Request-Type";
        /// <summary>
        /// It gives you the complete request data for the response
        /// </summary>
        public const string GiveMeFullRequestHeaderValue = "Full-Request";
        /// <summary>
        /// It gives you the complete last requested data for the response
        /// </summary>
        public const string GiveMeLastFullRequestHeaderValue = "Last-Full-Request";
    }
}
