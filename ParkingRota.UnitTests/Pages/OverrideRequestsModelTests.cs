namespace ParkingRota.UnitTests.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Security.Claims;
    using Moq;
    using NodaTime;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using ParkingRota.Pages;
    using Xunit;

    public static class OverrideRequestsModelTests
    {
        [Fact]
        public static void Test_Get()
        {
            var principal = new ClaimsPrincipal();
            var loggedInUser = Create.User("Colm Wilkinson");
            var otherUser = Create.User("Philip Quast");

            var applicationUsers = new[] { loggedInUser, otherUser };

            var mockUserManager = TestHelpers.CreateMockUserManager(principal, loggedInUser);
            mockUserManager
                .SetupGet(u => u.Users)
                .Returns(applicationUsers.AsQueryable());

            var model = new OverrideRequestsModel(mockUserManager.Object, Mock.Of<IRequestRepository>());

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

        [Fact]
        public static void Test_Post()
        {
            // Arrange
            var selectedUser = Create.User("Colm Wilkinson");

            // Set up request repository
            var mockRequestRepository = new Mock<IRequestRepository>(MockBehavior.Strict);
            mockRequestRepository
                .Setup(r => r.UpdateRequests(selectedUser, It.IsAny<IReadOnlyList<Request>>()));

            // Set up user manager
            var mockUserManager = TestHelpers.CreateMockUserManager(new[] { selectedUser });

            // Act
            var requestDates = new[] { 13.November(2018), 15.November(2018), 16.November(2018) };

            var model = new OverrideRequestsModel(mockUserManager.Object, mockRequestRepository.Object);

            model.OnPost(selectedUser.Id, requestDates.Select(d => d.ForRoundTrip()).ToArray());

            // Assert
            mockRequestRepository.Verify(
                r => r.UpdateRequests(selectedUser, It.Is(Match(selectedUser, requestDates))),
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