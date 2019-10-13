namespace ParkingRota
{
    using System.Linq;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;

    public static class ExtensionMethods
    {
        public static string GetOriginatingIpAddress(this IHttpContextAccessor httpContextAccessor)
        {
            const string CloudFlareConnectingIpHeaderKey = "CF-Connecting-IP";

            var cloudFlareConnectingIpHeader =
                httpContextAccessor.HttpContext.Request.Headers[CloudFlareConnectingIpHeaderKey].FirstOrDefault();

            return cloudFlareConnectingIpHeader ?? httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
        }
    }
}