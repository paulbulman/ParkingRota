namespace ParkingRota.UnitTests.Pages
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Moq;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using ParkingRota.Pages;
    using Xunit;

    public class EditRequestsModelTests
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
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            mockUserManager
                .Setup(u => u.GetUserAsync(principal))
                .Returns(Task.FromResult(loggedInUser));

            // Act
            var model = new EditRequestsModel(mockDateCalculator.Object, mockRequestRepository.Object, mockUserManager.Object)
            {
                PageContext = { HttpContext = new DefaultHttpContext { User = principal } }
            };

            await model.OnGetAsync();

            // Assert Calendar
            Assert.NotNull(model.Calendar);
            Assert.Single(model.Calendar.Weeks);
            Assert.Equal(5.November(2018), model.Calendar.Weeks[0].Days[0].Date);

            // Assert DisplayRequests
            Assert.NotNull(model.DisplayRequests);

            Assert.Equal(new[] { firstDate, lastDate }, model.DisplayRequests.Keys);

            Assert.True(model.DisplayRequests[firstDate]);
            Assert.False(model.DisplayRequests[lastDate]);
        }
    }
}