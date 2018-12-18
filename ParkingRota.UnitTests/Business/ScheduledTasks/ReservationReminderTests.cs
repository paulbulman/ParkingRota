namespace ParkingRota.UnitTests.Business.ScheduledTasks
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Moq;
    using NodaTime;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using ParkingRota.Business.ScheduledTasks;
    using Xunit;

    public static class ReservationReminderTests
    {
        [Fact]
        public static void Test_ScheduledTaskType()
        {
            var result = new ReservationReminder(
                Mock.Of<IDateCalculator>(),
                Mock.Of<IEmailRepository>(),
                Mock.Of<IReservationRepository>(),
                TestHelpers.CreateMockUserManager().Object).ScheduledTaskType;

            Assert.Equal(ScheduledTaskType.ReservationReminder, result);
        }

        [Fact]
        public static async Task Test_Run_NoReservationsEntered()
        {
            // Arrange
            var date = 14.December(2018);

            var mockDateCalculator = new Mock<IDateCalculator>(MockBehavior.Strict);
            mockDateCalculator
                .Setup(d => d.GetNextWorkingDate())
                .Returns(date);

            var mockReservationRepository = new Mock<IReservationRepository>(MockBehavior.Strict);
            mockReservationRepository
                .Setup(r => r.GetReservations(date, date))
                .Returns(new List<Reservation>());

            var mockEmailRepository = new Mock<IEmailRepository>(MockBehavior.Strict);
            mockEmailRepository
                .Setup(e => e.AddToQueue(It.IsAny<ParkingRota.Business.Emails.ReservationReminder>()));

            IList<ApplicationUser> teamLeaderUsers = new[]
            {
                new ApplicationUser { Email = "a@b.c" },
                new ApplicationUser { Email = "x@y.z" }
            };

            var mockUserManager = TestHelpers.CreateMockUserManager();
            mockUserManager
                .Setup(u => u.GetUsersInRoleAsync(UserRole.TeamLeader))
                .Returns(Task.FromResult(teamLeaderUsers));

            // Act
            var reservationReminder = new ReservationReminder(
                mockDateCalculator.Object,
                mockEmailRepository.Object,
                mockReservationRepository.Object,
                mockUserManager.Object);

            await reservationReminder.Run();

            // Assert
            foreach (var teamLeaderUser in teamLeaderUsers)
            {
                mockEmailRepository.Verify(
                    r => r.AddToQueue(
                        It.Is<ParkingRota.Business.Emails.ReservationReminder>(e => e.To == teamLeaderUser.Email)),
                    Times.Once);
            }
        }

        [Fact]
        public static async Task Test_Run_ReservationsAlreadyEntered()
        {
            // Arrange
            var date = 14.December(2018);

            var mockDateCalculator = new Mock<IDateCalculator>(MockBehavior.Strict);
            mockDateCalculator
                .Setup(d => d.GetNextWorkingDate())
                .Returns(date);

            var mockReservationRepository = new Mock<IReservationRepository>(MockBehavior.Strict);
            mockReservationRepository
                .Setup(r => r.GetReservations(date, date))
                .Returns(new[] { new Reservation() });

            var mockEmailRepository = new Mock<IEmailRepository>(MockBehavior.Strict).Object;

            // Act and assert: mock strict on email repository ensures nothing has been done.
            var reservationReminder = new ReservationReminder(
                mockDateCalculator.Object,
                mockEmailRepository,
                mockReservationRepository.Object,
                TestHelpers.CreateMockUserManager().Object);

            await reservationReminder.Run();
        }

        [Theory]
        [InlineData(22, 10, 23, 10)]
        [InlineData(23, 10, 26, 9)]
        public static void Test_GetNextRunTime(int currentDay, int currentHour, int expectedDay, int expectedHour)
        {
            // Arrange
            var mockDateCalculator = new Mock<IDateCalculator>(MockBehavior.Strict);
            mockDateCalculator
                .SetupGet(d => d.TimeZone)
                .Returns(DateTimeZoneProviders.Tzdb["Europe/London"]);
            mockDateCalculator
                .Setup(d => d.GetNextWorkingDate())
                .Returns(expectedDay.March(2018));

            var reservationReminder = new ReservationReminder(
                mockDateCalculator.Object,
                Mock.Of<IEmailRepository>(),
                Mock.Of<IReservationRepository>(),
                TestHelpers.CreateMockUserManager().Object);

            // Act
            var result = reservationReminder.GetNextRunTime(currentDay.March(2018).At(currentHour, 00, 00).Utc());

            // Assert
            var expected = expectedDay.March(2018).At(expectedHour, 00, 00).Utc();

            Assert.Equal(expected, result);
        }
    }
}