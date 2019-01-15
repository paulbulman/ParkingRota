namespace ParkingRota.IntegrationTests.Pages
{
    using System;
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
    using Allocation = Data.Allocation;
    using Request = Data.Request;

    public class SummaryTests : IClassFixture<DatabaseWebApplicationFactory<Program>>
    {
        private readonly DatabaseWebApplicationFactory<Program> factory;

        public SummaryTests(DatabaseWebApplicationFactory<Program> factory) => this.factory = factory;

        [Fact]
        public async Task Test_Index()
        {
            var summaryResponse = await this.CreateClient().LoadAuthenticatedPage("/Summary");

            Assert.Equal(HttpStatusCode.OK, summaryResponse.StatusCode);

            var summaryDocument = await HtmlHelpers.GetDocumentAsync(summaryResponse);

            var calendarTable = summaryDocument.QuerySelector("table");

            Assert.NotNull(calendarTable);
            Assert.IsAssignableFrom<IHtmlTableElement>(calendarTable);

            var rows = ((IHtmlTableElement)calendarTable).Rows;

            Assert.Equal(10, rows.Length);
            Assert.All(rows, r => Assert.Equal(5, r.Cells.Length));
            Assert.True(rows[1].Cells[2].InnerHtml.Contains("Anne Other", StringComparison.Ordinal));
            Assert.True(rows[1].Cells[3].InnerHtml.Contains("Jane Smith", StringComparison.Ordinal));
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

                        var otherUser = new ApplicationUser
                        {
                            FirstName = "Jane",
                            LastName = "Smith"
                        };

                        context.Users.Add(otherUser);

                        context.Requests.Add(new Request
                        {
                            ApplicationUser = applicationUser,
                            Date = 7.November(2018)
                        });

                        context.Requests.Add(new Request
                        {
                            ApplicationUser = otherUser,
                            Date = 8.November(2018)
                        });

                        context.Allocations.Add(new Allocation
                        {
                            ApplicationUser = applicationUser,
                            Date = 7.November(2018)
                        });

                        context.SaveChanges();
                    }
                });
            })
            .CreateClient();
    }
}