namespace ParkingRota.UnitTests.ViewComponents
{
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using Microsoft.AspNetCore.Mvc.ViewComponents;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using ParkingRota.ViewComponents;
    using Xunit;

    public class EditRequestsViewComponentTests : DatabaseTests
    {
        [Fact]
        public async Task Test_Invoke()
        {
            // Arrange
            var currentInstant = 6.November(2018).AtMidnight().Utc();
            this.SetClock(currentInstant);

            var loggedInUser = await this.Seed.ApplicationUser("a@b.c");
            var otherUser = await this.Seed.ApplicationUser("d@e.f");

            var bothUsersRequestDate = 7.November(2018);
            var otherUserRequestDate = 8.November(2018);

            this.Seed.Request(loggedInUser, bothUsersRequestDate);
            this.Seed.Request(otherUser, bothUsersRequestDate);
            this.Seed.Request(otherUser, otherUserRequestDate);

            using (var scope = this.CreateScope())
            {
                // Act
                var viewComponent = new EditRequestsViewComponent(
                    scope.ServiceProvider.GetRequiredService<IDateCalculator>(),
                    scope.ServiceProvider.GetRequiredService<IRequestRepository>());

                var result = (ViewViewComponentResult)viewComponent.Invoke(loggedInUser.Id);

                var viewModel = (EditRequestsViewComponent.EditRequestsViewModel)result.ViewData.Model;

                // Assert
                var expectedFirstActiveDate = 6.November(2018);
                var expectedLastActiveDate = 31.December(2018);

                Assert.Equal(loggedInUser.Id, viewModel.SelectedUserId);

                Assert.NotNull(viewModel.Calendar);

                Assert.Equal(5.November(2018), viewModel.Calendar.Weeks[0].Days[0].Date);

                Assert.Equal(expectedFirstActiveDate, viewModel.Calendar.ActiveDates().First());
                Assert.Equal(expectedLastActiveDate, viewModel.Calendar.ActiveDates().Last());

                Assert.True(viewModel.Calendar.Data(bothUsersRequestDate).IsSelected);
                Assert.False(viewModel.Calendar.Data(otherUserRequestDate).IsSelected);

                Assert.False(viewModel.Calendar.Data(bothUsersRequestDate).IsNextMonth);
                Assert.True(viewModel.Calendar.Data(expectedLastActiveDate).IsNextMonth);
            }
        }
    }
}