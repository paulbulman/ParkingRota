namespace ParkingRota.UnitTests.ViewComponents
{
    using System.Collections.Generic;
    using System.Linq;
    using Data;
    using Microsoft.AspNetCore.Mvc.ViewComponents;
    using NodaTime.Testing;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using ParkingRota.ViewComponents;
    using Xunit;
    using DataRequest = ParkingRota.Data.Request;

    public class EditRequestsViewComponentTests : DatabaseTests
    {
        [Fact]
        public void Test_Invoke()
        {
            // Arrange
            var loggedInUser = new ApplicationUser { FirstName = "Colm", LastName = "Wilkinson" };
            var otherUser = new ApplicationUser { FirstName = "Philip", LastName = "Quast" };

            var currentInstant = 6.November(2018).AtMidnight().Utc();

            var bothUsersRequestDate = 7.November(2018);
            var otherUserRequestDate = 8.November(2018);

            var requests = new[]
            {
                new DataRequest { ApplicationUser = loggedInUser, Date = bothUsersRequestDate},
                new DataRequest { ApplicationUser = otherUser, Date = bothUsersRequestDate},
                new DataRequest { ApplicationUser = otherUser, Date = otherUserRequestDate}
            };
            
            this.SeedDatabase(requests);

            using (var context = this.CreateContext())
            {
                // Act
                var viewComponent = new EditRequestsViewComponent(
                    new DateCalculator(new FakeClock(currentInstant), BankHolidayRepositoryTests.CreateRepository(context)),
                    new RequestRepositoryBuilder().WithCurrentInstant(currentInstant).Build(context));

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

        private void SeedDatabase(IReadOnlyList<DataRequest> requests)
        {
            using (var context = this.CreateContext())
            {
                context.Requests.AddRange(requests);
                context.SaveChanges();
            }
        }
    }
}