namespace ParkingRota.Middleware
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate next;
        private readonly SecurityHeadersPolicy policy;

        public SecurityHeadersMiddleware(RequestDelegate next, SecurityHeadersPolicy policy)
        {
            this.next = next;
            this.policy = policy;
        }

        public async Task Invoke(HttpContext context)
        {
            var headers = context.Response.Headers;

            foreach (var policyHeader in this.policy.Headers)
            {
                headers.Add(policyHeader.Key, policyHeader.Value);
            }

            await this.next(context);
        }
    }
}