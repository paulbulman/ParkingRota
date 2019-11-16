namespace ParkingRota.UnitTests.Business.ScheduledTasks
{
    using System.Collections.Generic;
    using System.Linq;
    using Data;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using ParkingRota.Business.ScheduledTasks;
    using ParkingRota.Data;
    using Xunit;

    public class WeeklySummaryTests : DatabaseTests
    {
        [Fact]
        public void Test_ScheduledTaskType()
        {
            using (var scope = this.CreateScope())
            {
                Assert.Equal(
                    ScheduledTaskType.WeeklySummary,
                    CreateWeeklySummary(scope).ScheduledTaskType);
            }
        }

        [Fact]
        public async void Test_Run()
        {
            // Arrange
            this.SetClock(13.December(2018).AtMidnight().Utc());

            var firstDate = 24.December(2018);
            var lastDate = 28.December(2018);

            var allocatedUser = await this.Seed.ApplicationUser("a@b.c");
            var interruptedUser = await this.Seed.ApplicationUser("x@y.z");

            this.Seed.Allocation(allocatedUser, firstDate);
            this.Seed.Allocation(allocatedUser, lastDate);

            this.Seed.Request(allocatedUser, firstDate, isAllocated: true);
            this.Seed.Request(allocatedUser, lastDate, isAllocated: true);
            this.Seed.Request(interruptedUser, firstDate, isAllocated: false);
            this.Seed.Request(interruptedUser, lastDate, isAllocated: false);

            // Act
            using (var scope = this.CreateScope())
            {
                await CreateWeeklySummary(scope).Run();
            }

            // Assert
            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                foreach (var applicationUser in new[] { allocatedUser, interruptedUser })
                {
                    var userEmails = context.EmailQueueItems.Where(e =>
                        e.To == applicationUser.Email &&
                        e.Subject == $"Weekly provisional allocations summary for {firstDate.ForDisplay()} - {lastDate.ForDisplay()}");

                    Assert.Single(userEmails);
                }
            }
        }

        [Fact]
        public async void Test_Run_ExcludesVisitorAccounts()
        {
            // Arrange
            this.SetClock(13.December(2018).AtMidnight().Utc());

            var firstDate = 24.December(2018);
            var lastDate = 28.December(2018);

            var visitorUser = await this.Seed.ApplicationUser("x@y.z", isVisitor: true);

            this.Seed.Request(visitorUser, firstDate, isAllocated: false);
            this.Seed.Request(visitorUser, lastDate, isAllocated: false);

            // Act
            using (var scope = this.CreateScope())
            {
                await CreateWeeklySummary(scope).Run();
            }

            // Assert
            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                Assert.Empty(context.EmailQueueItems);
            }
        }

        [Theory]
        [InlineData(15, 0, 22, 0)]
        [InlineData(16, 0, 22, 0)]
        [InlineData(22, 0, 28, 23)]
        public void Test_GetNextRunTime(int currentDay, int currentHour, int expectedDay, int expectedHour)
        {
            // Arrange
            var currentInstant = currentDay.March(2018).At(currentHour, 00, 00).Utc();
            this.SetClock(currentInstant);

            using (var scope = this.CreateScope())
            {
                // Act
                var result = CreateWeeklySummary(scope).GetNextRunTime(currentInstant);

                // Assert
                var expected = expectedDay.March(2018).At(expectedHour, 00, 00).Utc();

                Assert.Equal(expected, result);
            }
        }

        private static WeeklySummary CreateWeeklySummary(IServiceScope scope) =>
            scope.ServiceProvider
                .GetRequiredService<IEnumerable<IScheduledTask>>()
                .OfType<WeeklySummary>()
                .Single();
    }
}