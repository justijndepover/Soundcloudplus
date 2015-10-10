using System.Net;

namespace ClassLibrary.API
{
    public class ApiResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public dynamic Data { get; set; }

    }
}
