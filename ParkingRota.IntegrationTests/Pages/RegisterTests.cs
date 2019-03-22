namespace ParkingRota.IntegrationTests.Pages
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AngleSharp.Html.Dom;
    using Data;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime;
    using NodaTime.Testing;
    using NodaTime.Testing.Extensions;
    using UnitTests;
    using Xunit;

    public class RegisterTests : IClassFixture<DatabaseWebApplicationFactory<Program>>
    {
        private readonly DatabaseWebApplicationFactory<Program> factory;

        public RegisterTests(DatabaseWebApplicationFactory<Program> factory) => this.factory = factory;

        [Fact]
        public async Task Test_ValidRegistrationSucceeds()
        {
            const string RegistrationToken = "21273835-37E3-4839-AF9A-EFE13F485AAD";
            const string Password = "529064FD-a221-41C7-8BFC-0EC61B14BBDE";

            var client = this.CreateClient(RegistrationToken);
            var registerPage = await client.GetAsync("/Identity/Account/Register");

            Assert.Equal(HttpStatusCode.OK, registerPage.StatusCode);

            var content = await HtmlHelpers.GetDocumentAsync(registerPage);

            var form = (IHtmlFormElement)content.QuerySelector("form");

            var result = await client.SendAsync(form, CreateFormValues(Password, RegistrationToken));

            Assert.Equal(HttpStatusCode.Redirect, result.StatusCode);
            Assert.Equal("/RegisterSuccess", result.Headers.Location.OriginalString);
        }

        private HttpClient CreateClient(string registrationToken) =>
            this.factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var instant = 26.September(2018).At(22, 14, 03).Utc();

                    services.AddSingleton<IClock>(new FakeClock(instant));

                    using (var serviceScope = services.BuildServiceProvider().CreateScope())
                    {
                        var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        context.RegistrationTokens.Add(new RegistrationToken
                        {
                            Token = registrationToken,
                            ExpiryTime = instant.Plus(1.Seconds())
                        });

                        context.SaveChanges();
                    }
                });
            })
            .CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        private static Dictionary<string, string> CreateFormValues(string password, string registrationToken) =>
            new Dictionary<string, string>
            {
                { "Input.FirstName", "Anne" },
                { "Input.LastName", "Other" },
                { "Input.CarRegistrationNumber", "AB12CDE" },
                { "Input.AlternativeCarRegistrationNumber", "A123BCD" },
                { "Input.Email", "another@domain.com" },
                { "Input.Password", password },
                { "Input.ConfirmPassword", password },
                { "Input.RegistrationToken", registrationToken }
            };
    }
}