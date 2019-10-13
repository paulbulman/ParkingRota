namespace ParkingRota.UnitTests.Pages
{
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Data;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using ParkingRota.Data;
    using ParkingRota.Pages;
    using Xunit;

    public class EditRequestsModelTests : DatabaseTests
    {
        [Fact]
        public async Task Test_Post()
        {
            // Arrange
            var loggedInUser = await this.Seed.ApplicationUser("a@b.c");

            var principal = new ClaimsPrincipal(new[]
            {
                new ClaimsIdentity(new[] { new Claim(new ClaimsIdentityOptions().UserIdClaimType, loggedInUser.Id) })
            });

            // Act
            var requestDates = new[] { 13.November(2018), 15.November(2018), 16.November(2018) };

            using (var scope = this.CreateScope())
            {
                var requestRepository = scope.ServiceProvider.GetRequiredService<IRequestRepository>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                var model = new EditRequestsModel(requestRepository, userManager)
                {
                    PageContext = { HttpContext = new DefaultHttpContext { User = principal } }
                };

                await model.OnPostAsync(requestDates.Select(d => d.ForRoundTrip()).ToArray());

                Assert.Equal("Requests updated.", model.StatusMessage);
            }

            // Assert
            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var result = context.Requests
                    .Include(r => r.ApplicationUser)
                    .ToArray();

                Assert.Equal(requestDates.Length, result.Length);

                foreach (var requestDate in requestDates)
                {
                    Assert.Single(result.Where(r =>
                        r.ApplicationUser.Id == loggedInUser.Id &&
                        r.Date == requestDate));
                }
            }
        }
    }
}