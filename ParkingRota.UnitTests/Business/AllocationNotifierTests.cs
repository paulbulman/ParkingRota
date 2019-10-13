namespace ParkingRota.UnitTests.Business
{
    using System.Collections.Generic;
    using System.Linq;
    using Data;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using ParkingRota.Data;
    using Xunit;
    using Allocation = ParkingRota.Business.Model.Allocation;
    using DataScheduledTask = ParkingRota.Data.ScheduledTask;

    public class AllocationNotifierTests : DatabaseTests
    {
        [Fact]
        public void Test_Notify()
        {
            // Arrange
            var currentInstant = 27.December(2018).At(15, 48, 14).Utc();
            this.SetClock(currentInstant);

            var user = new ApplicationUser { Email = "a@b.c" };
            var otherUser = new ApplicationUser { Email = "x@y.z" };

            var allUsers = new[] { user, otherUser };

            var allocations = Create.Allocations(allUsers, 27.December(2018))
                .Concat(Create.Allocations(allUsers, 28.December(2018)))
                .ToArray();

            const bool DailySummaryDue = false;
            const bool WeeklySummaryDue = false;

            this.SetupScheduledTasks(currentInstant, DailySummaryDue, WeeklySummaryDue);

            // Act
            using (var scope = this.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<AllocationNotifier>().Notify(allocations);
            }

            // Assert
            this.CheckNotifiedAllocations(allocations);
        }

        [Fact]
        public void Test_Notify_DailySummaryDue()
        {
            // Arrange
            var currentInstant = 26.December(2018).At(11, 0, 1).Utc();
            this.SetClock(currentInstant);

            var user = new ApplicationUser { Email = "a@b.c" };
            var otherUser = new ApplicationUser { Email = "x@y.z" };

            var allUsers = new[] { user, otherUser };

            var nextDayAllocations = Create.Allocations(allUsers, 27.December(2018));
            var futureDayAllocations = Create.Allocations(allUsers, 28.December(2018));

            var allocations = nextDayAllocations
                .Concat(futureDayAllocations)
                .ToArray();

            const bool DailySummaryDue = true;
            const bool WeeklySummaryDue = false;

            this.SetupScheduledTasks(currentInstant, DailySummaryDue, WeeklySummaryDue);

            // Act
            using (var scope = this.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<AllocationNotifier>().Notify(allocations);
            }

            // Assert
            this.CheckNotifiedAllocations(futureDayAllocations);
        }

        [Fact]
        public void Test_Notify_WeeklySummaryDue()
        {
            // Arrange
            var currentInstant = 27.December(2018).At(0, 0, 1).Utc();
            this.SetClock(currentInstant);

            var user = new ApplicationUser { Email = "a@b.c" };
            var otherUser = new ApplicationUser { Email = "x@y.z" };

            var allUsers = new[] { user, otherUser };

            var nextDayAllocations = Create.Allocations(allUsers, 28.December(2018));
            var futureWeekAllocations =
                Create.Allocations(allUsers, 7.January(2019))
                    .Concat(Create.Allocations(allUsers, 7.January(2019)))
                    .Concat(Create.Allocations(allUsers, 11.January(2019)));

            var allocations = nextDayAllocations
                .Concat(futureWeekAllocations)
                .ToArray();

            const bool DailySummaryDue = false;
            const bool WeeklySummaryDue = true;

            this.SetupScheduledTasks(currentInstant, DailySummaryDue, WeeklySummaryDue);

            // Act
            using (var scope = this.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<AllocationNotifier>().Notify(allocations);
            }

            // Assert
            this.CheckNotifiedAllocations(nextDayAllocations);
        }

        [Fact]
        public void Test_Notify_ExcludesVisitorAccounts()
        {
            // Arrange
            var currentInstant = 27.December(2018).At(15, 48, 14).Utc();
            this.SetClock(currentInstant);

            var user = new ApplicationUser { Email = "a@b.c", IsVisitor = false };
            var visitorUser = new ApplicationUser { Email = "x@y.z", IsVisitor = true };

            var allUsers = new[] { user, visitorUser };

            var allocations = Create.Allocations(allUsers, 27.December(2018));

            const bool DailySummaryDue = false;
            const bool WeeklySummaryDue = false;

            this.SetupScheduledTasks(currentInstant, DailySummaryDue, WeeklySummaryDue);

            // Act
            using (var scope = this.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<AllocationNotifier>().Notify(allocations);
            }

            // Assert
            var expectedNotifiedAllocations = allocations.Where(a => a.ApplicationUser == user);

            this.CheckNotifiedAllocations(expectedNotifiedAllocations.ToArray());
        }

        private void SetupScheduledTasks(Instant currentInstant, bool dailySummaryDue, bool weeklySummaryDue)
        {
            var dailySummary = CreateScheduledTask(currentInstant, ScheduledTaskType.DailySummary, dailySummaryDue);
            var weeklySummary = CreateScheduledTask(currentInstant, ScheduledTaskType.WeeklySummary, weeklySummaryDue);

            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                context.ScheduledTasks.AddRange(dailySummary, weeklySummary);
                context.SaveChanges();
            }
        }

        private static DataScheduledTask CreateScheduledTask(Instant currentInstant, ScheduledTaskType scheduledTaskType, bool isDue)
        {
            var nextRunTimeOffset = isDue ? -1.Seconds() : 1.Seconds();

            return new DataScheduledTask
            {
                NextRunTime = currentInstant.Plus(nextRunTimeOffset),
                ScheduledTaskType = scheduledTaskType
            };
        }

        private void CheckNotifiedAllocations(IReadOnlyList<Allocation> expectedNotifiedAllocations)
        {
            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                Assert.Equal(expectedNotifiedAllocations.Count, context.EmailQueueItems.ToArray().Length);

                foreach (var allocation in expectedNotifiedAllocations)
                {
                    var allocationEmail = context.EmailQueueItems.Where(
                        e =>
                            e.To == allocation.ApplicationUser.Email &&
                            e.Subject == $"Space available on {allocation.Date.ForDisplay()}");

                    Assert.Single(allocationEmail);
                }
            }
        }
    }
}