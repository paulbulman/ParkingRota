namespace ParkingRota.Middleware
{
    using Microsoft.AspNetCore.Builder;

    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseResponseHeadersMiddleware(
            this IApplicationBuilder app,
            ResponseHeadersBuilder builder) => app.UseMiddleware<ResponseHeadersMiddleware>(builder.Build());
    }
}