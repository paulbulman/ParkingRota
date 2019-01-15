namespace ParkingRota.IntegrationTests.Pages
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AngleSharp.Dom.Html;
    using Data;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using ParkingRota.Business.Model;
    using Xunit;

    public class RegistrationNumbersTests : IClassFixture<DatabaseWebApplicationFactory<Program>>
    {
        private readonly DatabaseWebApplicationFactory<Program> factory;

        public RegistrationNumbersTests(DatabaseWebApplicationFactory<Program> factory) => this.factory = factory;

        [Fact]
        public async Task Test_Display()
        {
            var registrationNumbersResponse = await this.CreateClient().LoadAuthenticatedPage("/RegistrationNumbers");

            Assert.Equal(HttpStatusCode.OK, registrationNumbersResponse.StatusCode);

            var registrationNumbersDocument = await HtmlHelpers.GetDocumentAsync(registrationNumbersResponse);

            var registrationNumbersTable = registrationNumbersDocument.QuerySelector("table");

            Assert.NotNull(registrationNumbersTable);
            Assert.IsAssignableFrom<IHtmlTableElement>(registrationNumbersTable);

            var rows = ((IHtmlTableElement)registrationNumbersTable).Rows;

            Assert.Equal(3, rows.Length);
            Assert.All(rows, r => Assert.Equal(2, r.Cells.Length));

            Assert.True(rows[1].Cells[0].InnerHtml.Contains("AB12CDE", StringComparison.Ordinal));
            Assert.True(rows[1].Cells[1].InnerHtml.Contains("Jane Smith", StringComparison.Ordinal));

            Assert.True(rows[2].Cells[0].InnerHtml.Contains("W789XYZ", StringComparison.Ordinal));
            Assert.True(rows[2].Cells[1].InnerHtml.Contains("Anne Other", StringComparison.Ordinal));
        }

        private HttpClient CreateClient() =>
            this.factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    using (var serviceScope = services.BuildServiceProvider().CreateScope())
                    {
                        var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        var otherUser = new ApplicationUser
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            CarRegistrationNumber = "AB 12 CDE"
                        };

                        context.Users.Add(otherUser);

                        context.SaveChanges();
                    }
                });
            })
            .CreateClient();
    }
}