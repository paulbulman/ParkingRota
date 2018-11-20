namespace ParkingRota.UnitTests.ViewComponents
{
    using Moq;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using ParkingRota.ViewComponents;
    using Xunit;

    public class EditRequestsViewComponentTests
    {
        [Fact]
        public void Test_Invoke()
        {
            // Arrange
            var firstDate = 6.November(2018);
            var lastDate = 7.November(2018);

            var loggedInUser = new ApplicationUser { FirstName = "Colm", LastName = "Wilkinson" };
            var otherUser = new ApplicationUser { FirstName = "Philip", LastName = "Quast" };

            // Set up mock date calculator
            var mockDateCalculator = new Mock<IDateCalculator>(MockBehavior.Strict);
            mockDateCalculator
                .Setup(d => d.GetActiveDates())
                .Returns(new[] { firstDate, lastDate });

            // Set up request repository
            var mockRequestRepository = new Mock<IRequestRepository>(MockBehavior.Strict);
            mockRequestRepository
                .Setup(r => r.GetRequests(firstDate, lastDate))
                .Returns(new[]
                {
                    new Request { ApplicationUser = loggedInUser, Date = firstDate},
                    new Request { ApplicationUser = otherUser, Date = firstDate},
                    new Request { ApplicationUser = otherUser, Date = lastDate}
                });

            // Act
            var model = new EditRequestsViewComponent(mockDateCalculator.Object, mockRequestRepository.Object);

            model.Invoke(loggedInUser.Id);

            // Assert
            Assert.NotNull(model.Calendar);
            Assert.Single(model.Calendar.Weeks);
            Assert.Equal(5.November(2018), model.Calendar.Weeks[0].Days[0].Date);

            Assert.Equal(new[] { firstDate, lastDate }, model.Calendar.ActiveDates());

            Assert.True(model.Calendar.Data(firstDate));
            Assert.False(model.Calendar.Data(lastDate));
        }
    }
}