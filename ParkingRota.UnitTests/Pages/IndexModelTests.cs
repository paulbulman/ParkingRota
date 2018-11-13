namespace ParkingRota.UnitTests.Pages
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using ParkingRota.Pages;
    using Xunit;

    public class IndexModelTests
    {
        [Fact]
        public async Task Test_Get()
        {
            // Arrange
            var firstDate = 6.November(2018);
            var lastDate = 7.November(2018);

            var principal = new ClaimsPrincipal();
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

            // Set up user manager
            var mockUserManager = TestHelpers.CreateMockUserManager(principal, loggedInUser);

            // Act
            var model = new IndexModel(mockDateCalculator.Object, mockRequestRepository.Object, mockUserManager.Object)
            {
                PageContext = { HttpContext = new DefaultHttpContext { User = principal } }
            };

            await model.OnGetAsync();

            // Assert
            Assert.NotNull(model.Calendar);
            Assert.Single(model.Calendar.Weeks);
            Assert.Equal(5.November(2018), model.Calendar.Weeks[0].Days[0].Date);

            Assert.Equal(new[] { firstDate, lastDate }, model.Calendar.ActiveDates());

            Assert.Equal(2, model.Calendar.Data(firstDate).Count);

            Assert.Equal("Philip Quast", model.Calendar.Data(firstDate)[0].FullName);
            Assert.False(model.Calendar.Data(firstDate)[0].IsCurrentUser);
            Assert.Equal("Colm Wilkinson", model.Calendar.Data(firstDate)[1].FullName);
            Assert.True(model.Calendar.Data(firstDate)[1].IsCurrentUser);

            Assert.Equal(1, model.Calendar.Data(lastDate).Count);

            Assert.Equal("Philip Quast", model.Calendar.Data(lastDate)[0].FullName);
            Assert.False(model.Calendar.Data(lastDate)[0].IsCurrentUser);
        }
    }
}