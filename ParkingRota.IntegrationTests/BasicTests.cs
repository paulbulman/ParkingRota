namespace ParkingRota.IntegrationTests
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Xunit;

    public class BasicTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;

        public BasicTests(WebApplicationFactory<Program> factory) => this.factory = factory;

        [Theory]
        [InlineData("/")]
        [InlineData("/Identity/Account/Register")]
        [InlineData("/Identity/Account/ResetPassword?code=123")]
        public async Task Get_EndpointReturnsSuccessAndCorrectContentType(string requestUri)
        {
            // Arrange
            var client = this.factory.CreateClient();

            // Act
            var response = await client.GetAsync(requestUri);

            // Assert
            response.EnsureSuccessStatusCode();

            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }
    }
}
