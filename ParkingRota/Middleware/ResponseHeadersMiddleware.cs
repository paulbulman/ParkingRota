namespace ParkingRota.Middleware
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public class ResponseHeadersMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ResponseHeadersPolicy policy;

        public ResponseHeadersMiddleware(RequestDelegate next, ResponseHeadersPolicy policy)
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