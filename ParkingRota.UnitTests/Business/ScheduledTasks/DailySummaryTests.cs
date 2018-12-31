namespace ParkingRota.UnitTests.Business.ScheduledTasks
{
    using Moq;
    using NodaTime;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Emails;
    using ParkingRota.Business.Model;
    using Xunit;
    using DailySummary = ParkingRota.Business.ScheduledTasks.DailySummary;

    public static class DailySummaryTests
    {
        [Fact]
        public static void Test_ScheduledTaskType()
        {
            var result = new DailySummary(
                Mock.Of<IAllocationRepository>(),
                Mock.Of<IDateCalculator>(),
                Mock.Of<IEmailRepository>(),
                Mock.Of<IRequestRepository>()).ScheduledTaskType;

            Assert.Equal(ScheduledTaskType.DailySummary, result);
        }

        [Fact]
        public static async void Test_Run()
        {
            // Arrange
            var nextWorkingDate = 28.December(2018);

            var allocatedUser = new ApplicationUser { Email = "a@b.c" };
            var interruptedUser = new ApplicationUser { Email = "x@y.z" };

            var allocations = new[] { new Allocation { ApplicationUser = allocatedUser, Date = nextWorkingDate } };
            var requests = new[]
            {
                new Request { ApplicationUser = allocatedUser, Date = nextWorkingDate },
                new Request { ApplicationUser = interruptedUser, Date = nextWorkingDate }
            };

            var mockAllocationRepository = new Mock<IAllocationRepository>(MockBehavior.Strict);
            mockAllocationRepository
                .Setup(a => a.GetAllocations(nextWorkingDate, nextWorkingDate))
                .Returns(allocations);

            var mockDateCalculator = new Mock<IDateCalculator>(MockBehavior.Strict);
            mockDateCalculator
                .Setup(d => d.GetNextWorkingDate())
                .Returns(nextWorkingDate);

            var mockEmailRepository = new Mock<IEmailRepository>(MockBehavior.Strict);
            mockEmailRepository.Setup(e => e.AddToQueue(It.IsAny<IEmail>()));

            var mockRequestRepository = new Mock<IRequestRepository>(MockBehavior.Strict);
            mockRequestRepository
                .Setup(r => r.GetRequests(nextWorkingDate, nextWorkingDate))
                .Returns(requests);

            // Act
            var dailySummary = new DailySummary(
                mockAllocationRepository.Object,
                mockDateCalculator.Object,
                mockEmailRepository.Object,
                mockRequestRepository.Object);

            await dailySummary.Run();

            // Assert
            foreach (var applicationUser in new[] { allocatedUser, interruptedUser })
            {
                mockEmailRepository.Verify(
                    e => e.AddToQueue(
                        It.Is<ParkingRota.Business.Emails.DailySummary>(s => s.To == applicationUser.Email)),
                    Times.Once);
            }
        }

        [Theory]
        [InlineData(22, 11, 23, 11)]
        [InlineData(23, 11, 26, 10)]
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

            var reservationReminder = new DailySummary(
                Mock.Of<IAllocationRepository>(),
                mockDateCalculator.Object,
                Mock.Of<IEmailRepository>(),
                Mock.Of<IRequestRepository>());

            // Act
            var result = reservationReminder.GetNextRunTime(currentDay.March(2018).At(currentHour, 00, 00).Utc());

            // Assert
            var expected = expectedDay.March(2018).At(expectedHour, 00, 00).Utc();

            Assert.Equal(expected, result);
        }
    }
}