namespace ParkingRota.IntegrationTests.Pages
{
    using System.Threading.Tasks;
    using Xunit;

    public class RoleBasedAuthenticationTests : IClassFixture<DatabaseWebApplicationFactory<Program>>
    {
        private readonly DatabaseWebApplicationFactory<Program> factory;

        public RoleBasedAuthenticationTests(DatabaseWebApplicationFactory<Program> factory) => this.factory = factory;

        [Theory]
        [InlineData("/Users")]
        [InlineData("/Users/Create")]
        [InlineData("/EditReservations")]
        [InlineData("/OverrideRequests")]
        public async Task Test_RestrictedAuthenticatedPage(string requestUri)
        {
            var client = this.factory.CreateClient();

            var pageResponse = await client.LoadAuthenticatedPage(requestUri);

            var pageDocument = await HtmlHelpers.GetDocumentAsync(pageResponse);

            Assert.StartsWith("http://localhost/Identity/Account/AccessDenied", pageDocument.Url);
        }
    }
}