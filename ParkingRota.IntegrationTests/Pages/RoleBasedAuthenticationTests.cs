﻿namespace ParkingRota.IntegrationTests.Pages
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AngleSharp.Dom.Html;
    using Data;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using ParkingRota.Business.Model;
    using Xunit;

    public class RoleBasedAuthenticationTests : IClassFixture<DatabaseWebApplicationFactory<Program>>
    {
        private readonly DatabaseWebApplicationFactory<Program> factory;

        private const string EmailAddress = "anneother@gmail.com";
        private const string Password = "9Ft6M%";
        private const string PasswordHash =
            "AQAAAAEAACcQAAAAEGe/qgvKfGP5QOeQnC2YF5Fzphi2AvOD71xUXnzfW4yQfuuEGJ4qrdzt9bwESjN4Mw==";

        public RoleBasedAuthenticationTests(DatabaseWebApplicationFactory<Program> factory) => this.factory = factory;

        [Theory]
        [InlineData("/EditReservations")]
        [InlineData("/OverrideRequests")]
        public async Task Test_RestrictedAuthenticatedPage(string requestUri)
        {
            var client = this.CreateClient();

            var loginResponse = await client.GetAsync(requestUri);

            var loginDocument = await HtmlHelpers.GetDocumentAsync(loginResponse);

            var loginForm = (IHtmlFormElement)loginDocument.QuerySelector("form");

            var loginFormValues = new Dictionary<string, string>
            {
                { "Input.Email", EmailAddress },
                { "Input.Password", Password }
            };

            var pageResponse = await client.SendAsync(loginForm, loginFormValues);

            var pageDocument = await HtmlHelpers.GetDocumentAsync(pageResponse);

            Assert.StartsWith("http://localhost/Identity/Account/AccessDenied", pageDocument.Url);
        }

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
                                CarRegistrationNumber = "AB12CDE",
                                CommuteDistance = 9.99m,
                                FirstName = "Anne",
                                LastName = "Other",
                                EmailConfirmed = true
                            };

                            context.Users.Add(applicationUser);

                            context.SaveChanges();
                        }
                    });
                })
                .CreateClient();
    }
}