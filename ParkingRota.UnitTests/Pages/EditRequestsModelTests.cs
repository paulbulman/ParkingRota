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

            var model = new EditRequestsModel(mockRequestRepository.Object, mockUserManager.Object)
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