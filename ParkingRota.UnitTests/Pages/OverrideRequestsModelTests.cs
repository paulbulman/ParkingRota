namespace ParkingRota.UnitTests.Pages
{
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using ParkingRota.Data;
    using ParkingRota.Pages;
    using Xunit;

    public class OverrideRequestsModelTests : DatabaseTests
    {
        [Fact]
        public async Task Test_Get()
        {
            var loggedInUser = await this.Seed.ApplicationUser("Colm.Wilkinson@lesmis.com");
            var otherUser = await this.Seed.ApplicationUser("Philip.Quast@lesmis.com");

            var applicationUsers = new[]
            {
                loggedInUser,
                otherUser
            };

            using (var scope = this.CreateScope())
            {
                var model = CreateModel(scope);

                model.OnGet(loggedInUser.Id);

                Assert.Equal(loggedInUser.Id, model.SelectedUserId);

                Assert.Equal(applicationUsers.Length, model.Users.Count);

                Assert.Equal(
                    applicationUsers.OrderBy(u => u.LastName).Select(u => u.FullName),
                    model.Users.Select(u => u.Text));

                Assert.All(
                    applicationUsers,
                    u => Assert.Single(model.Users.Where(l => l.Value == u.Id && l.Text == u.FullName)));
            }
        }

        [Fact]
        public async Task Test_Post()
        {
            // Arrange
            var selectedUser = await this.Seed.ApplicationUser("a@b.c");

            // Act
            var requestDates = new[] { 13.November(2018), 15.November(2018), 16.November(2018) };

            using (var scope = this.CreateScope())
            {
                var model = CreateModel(scope);

                model.OnPost(selectedUser.Id, requestDates.Select(d => d.ForRoundTrip()).ToArray());

                Assert.Equal("Requests updated.", model.StatusMessage);
            }

            // Assert
            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var result = context.Requests.Include(a => a.ApplicationUser).ToArray();

                Assert.Equal(requestDates.Length, result.Length);

                foreach (var requestDate in requestDates)
                {
                    Assert.Single(result.Where(r =>
                        r.ApplicationUser.Id == selectedUser.Id &&
                        r.Date == requestDate));
                }
            }
        }

        private static OverrideRequestsModel CreateModel(IServiceScope scope)
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var requestRepository = scope.ServiceProvider.GetRequiredService<IRequestRepository>();

            return new OverrideRequestsModel(userManager, requestRepository);
        }
    }
}