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
    using DataRequest = ParkingRota.Data.Request;

    public class RequestReminderTests : DatabaseTests
    {
        [Fact]
        public void Test_ScheduledTaskType()
        {
            using (var scope = this.CreateScope())
            {
                Assert.Equal(
                    ScheduledTaskType.RequestReminder,
                    CreateRequestReminder(scope).ScheduledTaskType);
            }
        }

        [Fact]
        public async Task Test_Run_RequestsNotEntered()
        {
            // Arrange
            this.SetClock(12.December(2018).AtMidnight().Utc());

            var userWithRequests = await this.Seed.ApplicationUser("d@e.f");
            var userWithoutRequests = await this.Seed.ApplicationUser("a@b.c");
            var otherUserWithoutRequests = await this.Seed.ApplicationUser("x@y.z");

            var upcomingReminderDate = 28.December(2018);
            var previouslyRemindedDate = 21.December(2018);

            this.Seed.Request(userWithRequests, upcomingReminderDate, isAllocated: false);
            this.Seed.Request(userWithoutRequests, previouslyRemindedDate, isAllocated: false);
            this.Seed.Request(otherUserWithoutRequests, previouslyRemindedDate, isAllocated: false);

            // Act
            using (var scope = this.CreateScope())
            {
                await CreateRequestReminder(scope).Run();
            }

            // Assert
            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                foreach (var expectedApplicationUser in new[] { userWithoutRequests, otherUserWithoutRequests })
                {
                    var userEmails = context.EmailQueueItems.Where(e =>
                        e.To == expectedApplicationUser.Email &&
                        e.Subject == $"No requests entered for {24.December(2018).ForDisplay()} - {28.December(2018).ForDisplay()}");

                    Assert.Single(userEmails);
                }
            }
        }

        [Fact]
        public async Task Test_Run_RequestsAlreadyEntered()
        {
            // Arrange
            this.SetClock(12.December(2018).AtMidnight().Utc());

            var user = await this.Seed.ApplicationUser("d@e.f");

            var upcomingReminderDate = 28.December(2018);
            this.Seed.Request(user, upcomingReminderDate, isAllocated: false);

            // Act
            using (var scope = this.CreateScope())
            {
                await CreateRequestReminder(scope).Run();
            }

            // Assert
            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                Assert.Empty(context.EmailQueueItems);
            }
        }

        [Fact]
        public async Task Test_Run_InactiveUser()
        {
            // Arrange
            var date = 12.December(2018);
            this.SetClock(date.AtMidnight().Utc());

            var user = await this.Seed.ApplicationUser("d@e.f");

            this.Seed.Request(user, date.PlusDays(-31), isAllocated: false);

            // Act
            using (var scope = this.CreateScope())
            {
                await CreateRequestReminder(scope).Run();
            }

            // Assert
            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                Assert.Empty(context.EmailQueueItems);
            }
        }

        [Fact]
        public async Task Test_Run_VisitorUser()
        {
            // Arrange
            this.SetClock(12.December(2018).AtMidnight().Utc());

            var user = await this.Seed.ApplicationUser("d@e.f", isVisitor: true);

            var previouslyRemindedDate = 21.December(2018);

            this.Seed.Request(user, previouslyRemindedDate, isAllocated: false);

            // Act
            using (var scope = this.CreateScope())
            {
                await CreateRequestReminder(scope).Run();
            }

            // Assert
            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                Assert.Empty(context.EmailQueueItems);
            }
        }

        [Theory]
        [InlineData(14, 0, 21, 0)]
        [InlineData(15, 0, 21, 0)]
        [InlineData(21, 0, 27, 23)]
        public void Test_GetNextRunTime(int currentDay, int currentHour, int expectedDay, int expectedHour)
        {
            // Arrange
            var currentInstant = currentDay.March(2018).At(currentHour, 00, 00).Utc();
            this.SetClock(currentInstant);

            using (var scope = this.CreateScope())
            {
                // Act
                var result = CreateRequestReminder(scope).GetNextRunTime(currentInstant);

                // Assert
                var expected = expectedDay.March(2018).At(expectedHour, 00, 00).Utc();

                Assert.Equal(expected, result);
            }
        }

        private static RequestReminder CreateRequestReminder(IServiceScope scope) =>
            scope.ServiceProvider
                .GetRequiredService<IEnumerable<IScheduledTask>>()
                .OfType<RequestReminder>()
                .Single();
    }
}