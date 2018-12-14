namespace ParkingRota.UnitTests.Business.ScheduledTasks
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Moq;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using ParkingRota.Business.ScheduledTasks;
    using Xunit;

    public static class ReservationReminderTests
    {
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
                .Setup(e => e.AddToQueue(It.IsAny<ParkingRota.Business.Emails.ReservationsReminder>()));

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
            var reservationReminder = new ReservationsReminder(
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
                        It.Is<ParkingRota.Business.Emails.ReservationsReminder>(e => e.To == teamLeaderUser.Email)),
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
            var reservationReminder = new ReservationsReminder(
                mockDateCalculator.Object,
                mockEmailRepository,
                mockReservationRepository.Object,
                TestHelpers.CreateMockUserManager().Object);

            await reservationReminder.Run();
        }
    }
}