using System.Net;

namespace infrastructure.Helpers
{
    public static class StatusCodes
    {
        public static int AsInt(this HttpStatusCode statusCode)
        {
            return (int)statusCode;
        }
    }
}