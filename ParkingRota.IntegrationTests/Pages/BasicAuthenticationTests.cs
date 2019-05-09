namespace ParkingRota.IntegrationTests.Pages
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Xunit;

    public class BasicAuthenticationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;

        public BasicAuthenticationTests(WebApplicationFactory<Program> factory) => this.factory = factory;

        [Theory]
        [InlineData("/Summary")]
        [InlineData("/AddNewUser")]
        [InlineData("/EditRequests")]
        [InlineData("/EditReservations")]
        [InlineData("/OverrideRequests")]
        [InlineData("/RegistrationNumbers")]
        public async Task Test_AuthenticatedPage(string requestUri)
        {
            var response = await this.GetResponse(requestUri);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.StartsWith("http://localhost/Identity/Account/Login", response.Headers.Location.OriginalString);
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/Index")]
        [InlineData("/Error")]
        [InlineData("/Privacy")]
        public async Task Test_AnonymousPage(string requestUri)
        {
            var response = await this.GetResponse(requestUri);

            response.EnsureSuccessStatusCode();

            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }

        private async Task<HttpResponseMessage> GetResponse(string requestUri)
        {
            var client = this.factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });

            return await client.GetAsync(requestUri);
        }
    }
}