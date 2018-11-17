namespace ParkingRota.UnitTests.Pages
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using NodaTime;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using ParkingRota.Pages;
    using Xunit;

    public class EditReservationsModelTests
    {
        [Fact]
        public void Test_Get()
        {
            // Arrange
            var firstDate = 6.November(2018);
            var lastDate = 7.November(2018);

            var activeDates = new[] { firstDate, lastDate };

            var principal = new ClaimsPrincipal();
            var loggedInUser = new ApplicationUser { Id = "loggedInUser", FirstName = "Colm", LastName = "Wilkinson" };
            var otherUser = new ApplicationUser { Id = "otherUser", FirstName = "Philip", LastName = "Quast" };

            var applicationUsers = new[] { loggedInUser, otherUser };

            var systemParameterList = new SystemParameterList { ReservableSpaces = 3 };

            // Set up mock date calculator
            var mockDateCalculator = new Mock<IDateCalculator>(MockBehavior.Strict);
            mockDateCalculator
                .Setup(d => d.GetActiveDates())
                .Returns(activeDates);

            // Set up reservation repository
            var mockReservationRepository = new Mock<IReservationRepository>(MockBehavior.Strict);
            mockReservationRepository
                .Setup(r => r.GetReservations(firstDate, lastDate))
                .Returns(
                    new[]
                    {
                        new Reservation { ApplicationUser = loggedInUser, Date = firstDate, Order = 0 },
                        new Reservation { ApplicationUser = otherUser, Date = firstDate, Order = 1 },
                        new Reservation { ApplicationUser = otherUser, Date = lastDate, Order = 0 }
                    });

            // Set up system parameter list repository
            var mockSystemParameterListRepository = new Mock<ISystemParameterListRepository>(MockBehavior.Strict);
            mockSystemParameterListRepository
                .Setup(p => p.GetSystemParameterList())
                .Returns(systemParameterList);

            // Set up user manager
            var mockUserManager = TestHelpers.CreateMockUserManager(principal, loggedInUser);
            mockUserManager
                .SetupGet(u => u.Users)
                .Returns(applicationUsers.AsQueryable());

            // Act
            var model = new EditReservationsModel(
                mockDateCalculator.Object,
                mockSystemParameterListRepository.Object,
                mockReservationRepository.Object,
                mockUserManager.Object)
            {
                PageContext = { HttpContext = new DefaultHttpContext { User = principal } }
            };

            model.OnGet();

            // Assert
            Assert.NotNull(model.Calendar);
            Assert.Single(model.Calendar.Weeks);
            Assert.Equal(5.November(2018), model.Calendar.Weeks[0].Days[0].Date);

            Assert.Equal(activeDates, model.Calendar.ActiveDates());

            var expectedSelectedUserIds = new Dictionary<LocalDate, IReadOnlyList<string>>
            {
                { firstDate, new[] { loggedInUser.Id, otherUser.Id, null } },
                { lastDate, new[] { otherUser.Id, null, null } }
            };

            foreach (var activeDate in activeDates)
            {
                Assert.NotNull(model.Calendar.Data(activeDate).SpaceReservations);

                Assert.Equal(systemParameterList.ReservableSpaces, model.Calendar.Data(activeDate).SpaceReservations.Count);

                for (var order = 0; order < model.Calendar.Data(activeDate).SpaceReservations.Count; order++)
                {
                    var spaceReservation = model.Calendar.Data(activeDate).SpaceReservations[order];

                    var expectedDisplayValues = new[] { $"Space {order + 1}", otherUser.FullName, loggedInUser.FullName };
                    var expectedKeys = new[]
                    {
                        $"{activeDate.ForRoundTrip()}|{order}|",
                        $"{activeDate.ForRoundTrip()}|{order}|{otherUser.Id}",
                        $"{activeDate.ForRoundTrip()}|{order}|{loggedInUser.Id}"
                    };

                    Assert.Equal(expectedDisplayValues, spaceReservation.Options.Select(d => d.DisplayValue));
                    Assert.Equal(expectedKeys, spaceReservation.Options.Select(d => d.Key));

                    var expectedSelectedUserId = expectedSelectedUserIds[activeDate][order];
                    foreach (var displayReservation in spaceReservation.Options)
                    {

                        var expectedIsSelected =
                            expectedSelectedUserId != null &&
                            displayReservation.Key.EndsWith(expectedSelectedUserId);

                        Assert.Equal(expectedIsSelected, displayReservation.IsSelected);
                    }
                }
            }
        }
    }
}