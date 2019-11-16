namespace ParkingRota.UnitTests.Business.ScheduledTasks
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using ParkingRota.Business.ScheduledTasks;
    using ParkingRota.Data;
    using Xunit;
    using DataAllocation = ParkingRota.Data.Allocation;
    using DataRequest = ParkingRota.Data.Request;

    public class DailySummaryTests : DatabaseTests
    {
        [Fact]
        public void Test_ScheduledTaskType()
        {
            using (var scope = this.CreateScope())
            {
                Assert.Equal(
                    ScheduledTaskType.DailySummary,
                    CreateDailySummary(scope).ScheduledTaskType);
            }
        }

        [Fact]
        public async void Test_Run()
        {
            // Arrange
            this.SetClock(27.December(2018).At(11, 0, 0).Utc());

            var nextWorkingDate = 28.December(2018);

            var allocatedUser = await this.Seed.ApplicationUser("a@b.c");
            var interruptedUser = await this.Seed.ApplicationUser("x@y.z");

            this.Seed.Allocation(allocatedUser, nextWorkingDate);

            this.Seed.Request(allocatedUser, nextWorkingDate, isAllocated: true);
            this.Seed.Request(interruptedUser, nextWorkingDate, isAllocated: false);

            // Act
            using (var scope = this.CreateScope())
            {
                await CreateDailySummary(scope).Run();
            }

            // Assert
            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                foreach (var applicationUser in new[] { allocatedUser, interruptedUser })
                {
                    var userEmails = context.EmailQueueItems.Where(e =>
                        e.To == applicationUser.Email &&
                        e.Subject.Contains(nextWorkingDate.ForDisplay()));

                    Assert.Single(userEmails);
                }
            }
        }

        [Fact]
        public async Task Test_Run_ExcludesVisitorAccounts()
        {
            // Arrange
            this.SetClock(27.December(2018).At(11, 0, 0).Utc());
            var nextWorkingDate = 28.December(2018);

            var visitorUser = await this.Seed.ApplicationUser("x@y.z", isVisitor: true);

            this.Seed.Request(visitorUser, nextWorkingDate, isAllocated: false);

            // Act
            using (var scope = this.CreateScope())
            {
                await CreateDailySummary(scope).Run();
            }

            // Assert
            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                Assert.Empty(context.EmailQueueItems);
            }
        }

        [Theory]
        [InlineData(22, 11, 23, 11)]
        [InlineData(23, 11, 26, 10)]
        public void Test_GetNextRunTime(int currentDay, int currentHour, int expectedDay, int expectedHour)
        {
            // Arrange
            var currentInstant = currentDay.March(2018).At(currentHour, 00, 00).Utc();
            this.SetClock(currentInstant);

            using (var scope = this.CreateScope())
            {
                // Act
                var result = CreateDailySummary(scope).GetNextRunTime(currentInstant);

                // Assert
                var expected = expectedDay.March(2018).At(expectedHour, 00, 00).Utc();

                Assert.Equal(expected, result);
            }
        }

        private static DailySummary CreateDailySummary(IServiceScope scope) =>
            scope.ServiceProvider
                .GetRequiredService<IEnumerable<IScheduledTask>>()
                .OfType<DailySummary>()
                .Single();
    }
}