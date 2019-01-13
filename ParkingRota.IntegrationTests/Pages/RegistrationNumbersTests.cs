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

        private const string EmailAddress = "anneother@gmail.com";
        private const string Password = "9Ft6M%";
        private const string PasswordHash =
            "AQAAAAEAACcQAAAAEGe/qgvKfGP5QOeQnC2YF5Fzphi2AvOD71xUXnzfW4yQfuuEGJ4qrdzt9bwESjN4Mw==";

        public RegistrationNumbersTests(DatabaseWebApplicationFactory<Program> factory) => this.factory = factory;

        [Fact]
        public async Task Test_Display()
        {
            var registrationNumbersResponse = await LoadRegistrationNumbersPage(this.CreateClient());

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

        private static async Task<HttpResponseMessage> LoadRegistrationNumbersPage(HttpClient client) =>
            await client.LoadAuthenticatedPage("/RegistrationNumbers", EmailAddress, Password);

        private HttpClient CreateClient() =>
            this.factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    using (var serviceScope = services.BuildServiceProvider().CreateScope())
                    {
                        var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        var applicationUser = new ApplicationUser
                        {
                            Id = "b35d8fae-6e76-486d-9255-4ea5b68527b1",
                            UserName = EmailAddress,
                            NormalizedUserName = EmailAddress.ToUpper(),
                            Email = EmailAddress,
                            NormalizedEmail = EmailAddress.ToUpper(),
                            PasswordHash = PasswordHash,
                            SecurityStamp = "DI5SLUUOBZMZJ3ROV6CKOO673JJFF72E",
                            ConcurrencyStamp = "1837d1c1-393b-46ba-9397-578fca593f9d",
                            CarRegistrationNumber = "W 789 XYZ",
                            CommuteDistance = 9.99m,
                            FirstName = "Anne",
                            LastName = "Other",
                            EmailConfirmed = true
                        };

                        var otherUser = new ApplicationUser
                        {
                            FirstName = "Jane",
                            LastName = "Smith",
                            CarRegistrationNumber = "AB 12 CDE"
                        };

                        context.Users.Add(applicationUser);
                        context.Users.Add(otherUser);

                        context.SaveChanges();
                    }
                });
            })
            .CreateClient();
    }
}