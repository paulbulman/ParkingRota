namespace ParkingRota.IntegrationTests.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AngleSharp.Html.Dom;
    using Data;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class AddNewUserTests : IClassFixture<DatabaseWebApplicationFactory<Program>>
    {
        private readonly DatabaseWebApplicationFactory<Program> factory;

        public AddNewUserTests(DatabaseWebApplicationFactory<Program> factory) => this.factory = factory;

        [Fact]
        public async Task Test_AddNewUser()
        {
            var client = this.CreateClient();

            var getResponse = await client.LoadAuthenticatedPage("/AddNewUser");
            var getDocument = await HtmlHelpers.GetDocumentAsync(getResponse);

            var form = getDocument
                .QuerySelectorAll("form")
                .OfType<IHtmlFormElement>()
                .Single(f => !(f.Action ?? string.Empty).Contains("logout", StringComparison.OrdinalIgnoreCase));

            var formValues = new Dictionary<string, string>
            {
                { "Input.Email", "another@domain.com" },
                { "Input.ConfirmEmail", "another@domain.com" }
            };

            var postResponse = await client.SendAsync(form, formValues);

            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        }

        private HttpClient CreateClient() =>
            this.factory.WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        using (var serviceScope = services.BuildServiceProvider().CreateScope())
                        {
                            var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                            var applicationUser = context.Users.Single();

                            var siteAdminRole = new IdentityRole("SiteAdmin");

                            var userRole = new IdentityUserRole<string>
                            {
                                RoleId = siteAdminRole.Id,
                                UserId = applicationUser.Id
                            };

                            context.Roles.Add(siteAdminRole);
                            context.UserRoles.Add(userRole);

                            context.SaveChanges();
                        }
                    });
                })
                .CreateClient();
    }
}