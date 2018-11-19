namespace ParkingRota.IntegrationTests.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

    public class EditReservationsTests : IClassFixture<DatabaseWebApplicationFactory<Program>>
    {
        private readonly DatabaseWebApplicationFactory<Program> factory;

        private const int ReservableSpaces = 4;

        private const string UserId = "b35d8fae-6e76-486d-9255-4ea5b68527b1";
        private const string EmailAddress = "anneother@gmail.com";
        private const string Password = "9Ft6M%";
        private const string PasswordHash =
            "AQAAAAEAACcQAAAAEGe/qgvKfGP5QOeQnC2YF5Fzphi2AvOD71xUXnzfW4yQfuuEGJ4qrdzt9bwESjN4Mw==";

        public EditReservationsTests(DatabaseWebApplicationFactory<Program> factory) => this.factory = factory;

        [Fact]
        public async Task Test_EditReservations_Get()
        {
            var response = await this.LoadEditReservationsPage(this.CreateClient());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var document = await HtmlHelpers.GetDocumentAsync(response);

            var table = CheckCalendarTable(document);

            var dropdownLists = table.Rows[1].Cells[2].QuerySelectorAll("select");

            Assert.Equal(ReservableSpaces, dropdownLists.Length);

            Assert.All(dropdownLists, d => Assert.Equal(2, d.ChildElementCount));

            Assert.All(dropdownLists, d => Assert.True(d.Children[0].InnerHtml.Contains("Space", StringComparison.OrdinalIgnoreCase)));
            Assert.All(dropdownLists, d => Assert.True(d.Children[1].InnerHtml.Contains("Anne Other", StringComparison.OrdinalIgnoreCase)));

            Assert.True(((IHtmlOptionElement)dropdownLists[2].Children[1]).IsSelected);
        }

        [Fact]
        public async Task Test_EditReservations_Post()
        {
            const int NewOrder = 3;

            var client = this.CreateClient();

            var getResponse = await this.LoadEditReservationsPage(client);

            var getDocument = await HtmlHelpers.GetDocumentAsync(getResponse);

            var form = getDocument
                .QuerySelectorAll("form")
                .OfType<IHtmlFormElement>()
                .Single(f => !(f.Action ?? string.Empty).Contains("logout", StringComparison.OrdinalIgnoreCase));

            var formValues = new Dictionary<string, string>
            {
                { "selectedReservationStrings", $"2018-11-08|{NewOrder}|{UserId}" }
            };

            var postResponse = await client.SendAsync(form, formValues);

            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);

            var postDocument = await HtmlHelpers.GetDocumentAsync(postResponse);

            var table = CheckCalendarTable(postDocument);

            var dropdownLists = table.Rows[1].Cells[NewOrder].QuerySelectorAll("select");

            Assert.True(((IHtmlOptionElement)dropdownLists[3].Children[1]).IsSelected);
        }

        private static IHtmlTableElement CheckCalendarTable(IHtmlDocument document)
        {
            var calendarTable = document.QuerySelector("table");

            Assert.NotNull(calendarTable);
            Assert.IsAssignableFrom<IHtmlTableElement>(calendarTable);

            var rows = ((IHtmlTableElement)calendarTable).Rows;

            Assert.Equal(10, rows.Length);

            Assert.All(rows, r => Assert.Equal(5, r.Cells.Length));

            return (IHtmlTableElement)calendarTable;
        }

        private async Task<HttpResponseMessage> LoadEditReservationsPage(HttpClient client)
        {
            var loginResponse = await client.GetAsync("/EditReservations");

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
                            Id = UserId,
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

                        var reservation = new Data.Reservation
                        {
                            ApplicationUser = applicationUser,
                            Date = 7.November(2018),
                            Order = 2
                        };

                        var systemParameterList = new Data.SystemParameterList
                        {
                            ReservableSpaces = ReservableSpaces
                        };

                        context.Users.Add(applicationUser);
                        context.Reservations.Add(reservation);
                        context.SystemParameterLists.Add(systemParameterList);

                        context.SaveChanges();
                    }
                });
            })
            .CreateClient();
    }
}