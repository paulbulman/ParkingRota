namespace ParkingRota.UnitTests.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using NodaTime;
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
            var mockUserManager = TestHelpers.CreateMockUserManager(principal, loggedInUser);

            // Act
            var model = new EditRequestsModel(mockDateCalculator.Object, mockRequestRepository.Object, mockUserManager.Object)
            {
                PageContext = { HttpContext = new DefaultHttpContext { User = principal } }
            };

            await model.OnGetAsync();

            // Assert
            Assert.NotNull(model.Calendar);
            Assert.Single(model.Calendar.Weeks);
            Assert.Equal(5.November(2018), model.Calendar.Weeks[0].Days[0].Date);

            Assert.Equal(new[] { firstDate, lastDate }, model.Calendar.ActiveDates());

            Assert.True(model.Calendar.Data(firstDate));
            Assert.False(model.Calendar.Data(lastDate));
        }

        [Fact]
        public async Task Test_Post()
        {
            // Arrange
            var principal = new ClaimsPrincipal();
            var loggedInUser = new ApplicationUser { FirstName = "Colm", LastName = "Wilkinson" };

            // Set up request repository
            var mockRequestRepository = new Mock<IRequestRepository>(MockBehavior.Strict);
            mockRequestRepository
                .Setup(r => r.UpdateRequests(loggedInUser, It.IsAny<IReadOnlyList<Request>>()));

            // Set up user manager
            var mockUserManager = TestHelpers.CreateMockUserManager(principal, loggedInUser);

            // Act
            var requestDates = new[] { 13.November(2018), 15.November(2018), 16.November(2018) };

            var model = new EditRequestsModel(Mock.Of<IDateCalculator>(), mockRequestRepository.Object, mockUserManager.Object)
            {
                PageContext = { HttpContext = new DefaultHttpContext { User = principal } }
            };

            await model.OnPostAsync(requestDates.Select(d => d.ForRoundTrip()).ToArray());

            // Assert
            mockRequestRepository.Verify(
                r => r.UpdateRequests(loggedInUser, It.Is(Match(loggedInUser, requestDates))),
                Times.Once);
        }

        private static Expression<Func<IReadOnlyList<Request>, bool>> Match(
            ApplicationUser expectedUser, IReadOnlyList<LocalDate> expectedDates) =>
            list =>
                list != null &&
                list.All(r => r.ApplicationUser == expectedUser) &&
                list.Select(r => r.Date).SequenceEqual(expectedDates);
    }
}