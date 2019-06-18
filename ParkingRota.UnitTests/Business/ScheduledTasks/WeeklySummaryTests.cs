namespace ParkingRota.UnitTests.Business.ScheduledTasks
{
    using System.Collections.Generic;
    using Moq;
    using NodaTime;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Emails;
    using ParkingRota.Business.Model;
    using Xunit;
    using WeeklySummary = ParkingRota.Business.ScheduledTasks.WeeklySummary;

    public static class WeeklySummaryTests
    {
        [Fact]
        public static void Test_ScheduledTaskType()
        {
            var result = new WeeklySummary(
                Mock.Of<IAllocationRepository>(),
                Mock.Of<IDateCalculator>(),
                Mock.Of<IEmailRepository>(),
                Mock.Of<IRequestRepository>()).ScheduledTaskType;

            Assert.Equal(ScheduledTaskType.WeeklySummary, result);
        }

        [Fact]
        public static async void Test_Run()
        {
            // Arrange
            var firstDate = 24.December(2018);
            var lastDate = 28.December(2018);

            var allocatedUser = new ApplicationUser { Email = "a@b.c" };
            var interruptedUser = new ApplicationUser { Email = "x@y.z" };

            var allocations = new[]
            {
                new Allocation { ApplicationUser = allocatedUser, Date = firstDate },
                new Allocation { ApplicationUser = allocatedUser, Date = lastDate }
            };

            var requests = new[]
            {
                new Request { ApplicationUser = allocatedUser, Date = firstDate },
                new Request { ApplicationUser = allocatedUser, Date = lastDate },
                new Request { ApplicationUser = interruptedUser, Date = firstDate },
                new Request { ApplicationUser = interruptedUser, Date = lastDate }
            };

            var mockAllocationRepository = new Mock<IAllocationRepository>(MockBehavior.Strict);
            mockAllocationRepository
                .Setup(a => a.GetAllocations(firstDate, lastDate))
                .Returns(allocations);

            var mockDateCalculator = new Mock<IDateCalculator>(MockBehavior.Strict);
            mockDateCalculator
                .Setup(d => d.GetWeeklySummaryDates())
                .Returns(new[] { firstDate, lastDate });

            var mockEmailRepository = new Mock<IEmailRepository>(MockBehavior.Strict);
            mockEmailRepository.Setup(e => e.AddToQueue(It.IsAny<IEmail>()));

            var mockRequestRepository = new Mock<IRequestRepository>(MockBehavior.Strict);
            mockRequestRepository
                .Setup(r => r.GetRequests(firstDate, lastDate))
                .Returns(requests);

            // Act
            var weeklySummary = new WeeklySummary(
                mockAllocationRepository.Object,
                mockDateCalculator.Object,
                mockEmailRepository.Object,
                mockRequestRepository.Object);

            await weeklySummary.Run();

            // Assert
            foreach (var applicationUser in new[] { allocatedUser, interruptedUser })
            {
                mockEmailRepository.Verify(
                    e => e.AddToQueue(
                        It.Is<ParkingRota.Business.Emails.WeeklySummary>(s => s.To == applicationUser.Email)),
                    Times.Once);
            }
        }

        [Fact]
        public static async void Test_Run_ExcludesVisitorAccounts()
        {
            // Arrange
            var firstDate = 24.December(2018);
            var lastDate = 28.December(2018);

            var visitorUser = new ApplicationUser { Email = "x@y.z", IsVisitor = true };

            var requests = new[]
            {
                new Request { ApplicationUser = visitorUser, Date = firstDate },
                new Request { ApplicationUser = visitorUser, Date = lastDate }
            };

            var mockAllocationRepository = new Mock<IAllocationRepository>(MockBehavior.Strict);
            mockAllocationRepository
                .Setup(a => a.GetAllocations(firstDate, lastDate))
                .Returns(new List<Allocation>());

            var mockDateCalculator = new Mock<IDateCalculator>(MockBehavior.Strict);
            mockDateCalculator
                .Setup(d => d.GetWeeklySummaryDates())
                .Returns(new[] { firstDate, lastDate });

            var mockEmailRepository = new Mock<IEmailRepository>(MockBehavior.Strict);

            var mockRequestRepository = new Mock<IRequestRepository>(MockBehavior.Strict);
            mockRequestRepository
                .Setup(r => r.GetRequests(firstDate, lastDate))
                .Returns(requests);

            // Act/Assert (Mock.Strict ensures no emails were sent)
            var weeklySummary = new WeeklySummary(
                mockAllocationRepository.Object,
                mockDateCalculator.Object,
                mockEmailRepository.Object,
                mockRequestRepository.Object);

            await weeklySummary.Run();
        }

        [Theory]
        [InlineData(15, 0, 22, 0)]
        [InlineData(16, 0, 22, 0)]
        [InlineData(22, 0, 28, 23)]
        public static void Test_GetNextRunTime(int currentDay, int currentHour, int expectedDay, int expectedHour)
        {
            // Arrange
            var mockDateCalculator = new Mock<IDateCalculator>(MockBehavior.Strict);
            mockDateCalculator
                .SetupGet(d => d.TimeZone)
                .Returns(DateTimeZoneProviders.Tzdb["Europe/London"]);

            var requestReminder = new WeeklySummary(
                Mock.Of<IAllocationRepository>(),
                mockDateCalculator.Object,
                Mock.Of<IEmailRepository>(),
                Mock.Of<IRequestRepository>());

            // Act
            var result = requestReminder.GetNextRunTime(currentDay.March(2018).At(currentHour, 00, 00).Utc());

            // Assert
            var expected = expectedDay.March(2018).At(expectedHour, 00, 00).Utc();

            Assert.Equal(expected, result);
        }
    }
}