namespace ParkingRota.IntegrationTests.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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
    using UnitTests;
    using Xunit;

    public class EditRequestsTests : IClassFixture<DatabaseWebApplicationFactory<Program>>
    {
        private readonly DatabaseWebApplicationFactory<Program> factory;

        public EditRequestsTests(DatabaseWebApplicationFactory<Program> factory) => this.factory = factory;

        [Fact]
        public async Task Test_EditRequests_Get()
        {
            var response = await this.CreateClient().LoadAuthenticatedPage("/EditRequests");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var document = await HtmlHelpers.GetDocumentAsync(response);

            var table = CheckCalendarTable(document);

            CheckCheckboxValue(table.Rows[1].Cells[2], expectIsChecked: true);
            CheckCheckboxValue(table.Rows[1].Cells[3], expectIsChecked: false);
        }

        [Fact]
        public async Task Test_EditRequests_Post()
        {
            var client = this.CreateClient();

            var getResponse = await client.LoadAuthenticatedPage("/EditRequests");

            var getDocument = await HtmlHelpers.GetDocumentAsync(getResponse);

            var form = getDocument
                .QuerySelectorAll("form")
                .OfType<IHtmlFormElement>()
                .Single(f => !(f.Action ?? string.Empty).Contains("logout", StringComparison.OrdinalIgnoreCase));

            var formValues = new Dictionary<string, string>
            {
                { "selectedDateStrings", "2018-11-12" }
            };

            var postResponse = await client.SendAsync(form, formValues);

            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);

            var postDocument = await HtmlHelpers.GetDocumentAsync(postResponse);

            var table = CheckCalendarTable(postDocument);

            CheckCheckboxValue(table.Rows[2].Cells[0], expectIsChecked: true);
            CheckCheckboxValue(table.Rows[2].Cells[1], expectIsChecked: false);
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

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
        private static void CheckCheckboxValue(IHtmlTableCellElement cell, bool expectIsChecked)
        {
            var checkbox = cell.QuerySelector("input");

            Assert.NotNull(checkbox);

            Assert.IsAssignableFrom<IHtmlInputElement>(checkbox);

            Assert.Equal(expectIsChecked, ((IHtmlInputElement)checkbox).IsChecked);
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

                        var applicationUser = context.Users.Single();

                        var request = new Request
                        {
                            ApplicationUser = applicationUser,
                            Date = 7.November(2018)
                        };

                        context.Requests.Add(request);

                        context.SaveChanges();
                    }
                });
            })
            .CreateClient();
    }
}