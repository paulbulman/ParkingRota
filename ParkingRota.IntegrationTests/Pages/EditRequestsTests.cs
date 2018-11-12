namespace ParkingRota.IntegrationTests.Pages
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AngleSharp.Dom.Html;
    using Data;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime;
    using NodaTime.Testing;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.Model;
    using UnitTests;
    using Xunit;

    public class EditRequestsTests : IClassFixture<DatabaseWebApplicationFactory<Program>>
    {
        private readonly DatabaseWebApplicationFactory<Program> factory;

        private const string EmailAddress = "anneother@gmail.com";
        private const string Password = "9Ft6M%";
        private const string PasswordHash =
            "AQAAAAEAACcQAAAAEGe/qgvKfGP5QOeQnC2YF5Fzphi2AvOD71xUXnzfW4yQfuuEGJ4qrdzt9bwESjN4Mw==";

        public EditRequestsTests(DatabaseWebApplicationFactory<Program> factory) => this.factory = factory;

        [Fact]
        public async Task Test_EditRequests_Get()
        {
            var indexResponse = await this.LoadEditRequestsPage();

            Assert.Equal(HttpStatusCode.OK, indexResponse.StatusCode);

            var indexDocument = await HtmlHelpers.GetDocumentAsync(indexResponse);

            var calendarTable = indexDocument.QuerySelector("table");

            Assert.NotNull(calendarTable);
            Assert.IsAssignableFrom<IHtmlTableElement>(calendarTable);

            var rows = ((IHtmlTableElement)calendarTable).Rows;

            Assert.Equal(10, rows.Length);

            Assert.All(rows, r => Assert.Equal(5, r.Cells.Length));

            CheckCheckboxValue(rows[1].Cells[2], expectIsChecked: true);
            CheckCheckboxValue(rows[1].Cells[3], expectIsChecked: false);
        }

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
        private static void CheckCheckboxValue(IHtmlTableCellElement cell, bool expectIsChecked)
        {
            var checkbox = cell.QuerySelector("input");

            Assert.NotNull(checkbox);

            Assert.IsAssignableFrom<IHtmlInputElement>(checkbox);

            Assert.Equal(expectIsChecked, ((IHtmlInputElement)checkbox).IsChecked);
        }

        private async Task<HttpResponseMessage> LoadEditRequestsPage()
        {
            var client = this.CreateClient();

            var loginResponse = await client.GetAsync("/EditRequests");

            var loginDocument = await HtmlHelpers.GetDocumentAsync(loginResponse);

            var loginForm = (IHtmlFormElement)loginDocument.QuerySelector("form");

            var loginFormValues = new Dictionary<string, string>
            {
                { "Input.Email", EmailAddress },
                { "Input.Password", Password }
            };

            return await client.SendAsync(loginForm, loginFormValues);
        }

        private HttpClient CreateClient() =>
            this.factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var instant = 7.November(2018).At(20, 14, 03).Utc();

                    services.AddSingleton<IClock>(new FakeClock(instant));

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
                            CarRegistrationNumber = "AB12CDE",
                            CommuteDistance = 9.99m,
                            FirstName = "Anne",
                            LastName = "Other"
                        };

                        var request = new Data.Request
                        {
                            Id = 1,
                            ApplicationUser = applicationUser,
                            Date = 7.November(2018)
                        };

                        context.Users.Add(applicationUser);
                        context.Requests.Add(request);

                        context.SaveChanges();
                    }
                });
            })
            .CreateClient();
    }
}