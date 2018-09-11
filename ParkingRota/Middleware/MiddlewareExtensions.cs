namespace ParkingRota.Middleware
{
    using Microsoft.AspNetCore.Builder;

    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityHeadersMiddleware(
            this IApplicationBuilder app,
            SecurityHeadersBuilder builder)
        {
            return app.UseMiddleware<SecurityHeadersMiddleware>(builder.Build());
        }
    }
}