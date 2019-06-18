namespace ParkingRota.UnitTests.Business
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Moq;
    using NodaTime;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Emails;
    using ParkingRota.Business.Model;
    using Xunit;

    public static class AllocationNotifierTests
    {
        private static readonly Instant CurrentInstant = 27.December(2018).At(15, 48, 14).Utc();

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public static void Test_Notify(bool dailySummaryDue, bool weeklySummaryDue)
        {
            // Arrange
            var mockDateCalculator = new Mock<IDateCalculator>(MockBehavior.Strict);
            mockDateCalculator.SetupGet(d => d.CurrentInstant).Returns(CurrentInstant);

            if (dailySummaryDue)
            {
                mockDateCalculator
                    .Setup(d => d.GetNextWorkingDate())
                    .Returns(28.December(2018));
            }

            if (weeklySummaryDue)
            {
                mockDateCalculator
                    .Setup(d => d.GetWeeklySummaryDates())
                    .Returns(new[] { 31.December(2018), 2.January(2019), 3.January(2019), 4.January(2019) });
            }

            var mockScheduledTaskRepository = CreateMockScheduledTaskRepository(dailySummaryDue, weeklySummaryDue);

            var mockEmailRepository = new Mock<IEmailRepository>(MockBehavior.Strict);
            mockEmailRepository.Setup(e => e.AddToQueue(It.IsAny<IEmail>()));

            var user = new ApplicationUser { Email = "a@b.c" };
            var otherUser = new ApplicationUser { Email = "x@y.z" };

            var allUsers = new[] { user, otherUser };

            var alwaysNotifiedAllocations = Create.Allocations(allUsers, 27.December(2018));

            var dailySummaryAllocations = Create.Allocations(allUsers, 28.December(2018));

            var weeklySummaryAllocations = new[]
            {
                new Allocation { ApplicationUser = user, Date = 31.December(2018) },
                new Allocation { ApplicationUser = user, Date = 2.January(2019) },
                new Allocation { ApplicationUser = user, Date = 4.January(2019) },
                new Allocation { ApplicationUser = otherUser, Date = 2.January(2019) },
                new Allocation { ApplicationUser = otherUser, Date = 4.January(2019) }
            };

            // Act
            var allocationNotifier = new AllocationNotifier(
                mockDateCalculator.Object,
                mockEmailRepository.Object,
                mockScheduledTaskRepository.Object);

            var allAllocations = alwaysNotifiedAllocations
                .Concat(dailySummaryAllocations)
                .Concat(weeklySummaryAllocations)
                .ToArray();

            allocationNotifier.Notify(allAllocations);

            // Assert
            var expectedNotifiedAllocations = new List<Allocation>(alwaysNotifiedAllocations);

            if (!dailySummaryDue)
            {
                expectedNotifiedAllocations.AddRange(dailySummaryAllocations);
            }

            if (!weeklySummaryDue)
            {
                expectedNotifiedAllocations.AddRange(weeklySummaryAllocations);
            }

            mockEmailRepository.Verify(
                e => e.AddToQueue(It.IsAny<SingleAllocation>()),
                Times.Exactly(expectedNotifiedAllocations.Count));

            foreach (var expectedNotifiedAllocation in expectedNotifiedAllocations)
            {
                mockEmailRepository.Verify(
                    e => e.AddToQueue(It.Is<SingleAllocation>(s => Match(s, expectedNotifiedAllocation))),
                    Times.Once);
            }
        }

        [Fact]
        public static void Test_Notify_ExcludesVisitorAccounts()
        {
            // Arrange
            var mockDateCalculator = new Mock<IDateCalculator>(MockBehavior.Strict);
            mockDateCalculator.SetupGet(d => d.CurrentInstant).Returns(CurrentInstant);

            var mockScheduledTaskRepository =
                CreateMockScheduledTaskRepository(dailySummaryDue: false, weeklySummaryDue: false);

            var mockEmailRepository = new Mock<IEmailRepository>(MockBehavior.Strict);
            mockEmailRepository.Setup(e => e.AddToQueue(It.IsAny<IEmail>()));

            var user = new ApplicationUser { Email = "a@b.c", IsVisitor = false };
            var visitorUser = new ApplicationUser { Email = "x@y.z", IsVisitor = true };

            var allUsers = new[] { user, visitorUser };

            // Act
            var allocationNotifier = new AllocationNotifier(
                mockDateCalculator.Object,
                mockEmailRepository.Object,
                mockScheduledTaskRepository.Object);

            var allocations = Create.Allocations(allUsers, 27.December(2018)).ToArray();

            allocationNotifier.Notify(allocations);

            // Assert
            var expectedNotifiedAllocations = allocations
                .Where(a => a.ApplicationUser == user)
                .ToArray();

            mockEmailRepository.Verify(
                e => e.AddToQueue(It.IsAny<SingleAllocation>()),
                Times.Exactly(expectedNotifiedAllocations.Length));

            foreach (var expectedNotifiedAllocation in expectedNotifiedAllocations)
            {
                mockEmailRepository.Verify(
                    e => e.AddToQueue(It.Is<SingleAllocation>(s => Match(s, expectedNotifiedAllocation))),
                    Times.Once);
            }
        }

        private static Mock<IScheduledTaskRepository> CreateMockScheduledTaskRepository(
            bool dailySummaryDue,
            bool weeklySummaryDue)
        {
            var dailySummaryTask = CreateScheduledTask(ScheduledTaskType.DailySummary, dailySummaryDue);
            var weeklySummaryTask = CreateScheduledTask(ScheduledTaskType.WeeklySummary, weeklySummaryDue);

            var mockScheduledTaskRepository = new Mock<IScheduledTaskRepository>(MockBehavior.Strict);
            mockScheduledTaskRepository
                .Setup(s => s.GetScheduledTasks())
                .Returns(new[] {dailySummaryTask, weeklySummaryTask});

            return mockScheduledTaskRepository;
        }

        private static bool Match(IEmail email, Allocation expectedNotifiedAllocation) =>
            email.To == expectedNotifiedAllocation.ApplicationUser.Email &&
            email.Subject.Contains(expectedNotifiedAllocation.Date.ForDisplay(), StringComparison.OrdinalIgnoreCase);

        private static ScheduledTask CreateScheduledTask(ScheduledTaskType scheduledTaskType, bool isDue) =>
            new ScheduledTask
            {
                NextRunTime = isDue ? CurrentInstant : CurrentInstant.Plus(1.Seconds()),
                ScheduledTaskType = scheduledTaskType
            };
    }
}