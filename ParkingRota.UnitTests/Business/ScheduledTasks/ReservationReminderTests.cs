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

    public class ReservationReminderTests : DatabaseTests
    {
        [Fact]
        public void Test_ScheduledTaskType()
        {
            using (var scope = this.CreateScope())
            {
                Assert.Equal(
                    ScheduledTaskType.ReservationReminder,
                    CreateReservationReminder(scope).ScheduledTaskType);
            }
        }

        [Fact]
        public async Task Test_Run_NoReservationsEntered()
        {
            // Arrange
            var date = 13.December(2018);
            this.SetClock(date.At(10, 0, 0).Utc());

            var teamLeaderUsers = new[]
            {
                await this.Seed.ApplicationUser("a@b.c", isTeamLeader: true),
                await this.Seed.ApplicationUser("x@y.z", isTeamLeader: true)
            };

            // Act
            using (var scope = this.CreateScope())
            {
                await CreateReservationReminder(scope).Run();
            }

            // Assert
            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                foreach (var teamLeaderUser in teamLeaderUsers)
                {
                    var userEmails = context.EmailQueueItems.Where(e =>
                        e.To == teamLeaderUser.Email &&
                        e.Subject == $"No reservations entered for {date.PlusDays(1).ForDisplay()}");

                    Assert.Single(userEmails);
                }
            }
        }

        [Fact]
        public async Task Test_Run_ReservationsAlreadyEntered()
        {
            // Arrange
            var date = 13.December(2018);
            this.SetClock(date.At(10, 0, 0).Utc());

            var teamLeaderUsers = new[]
            {
                await this.Seed.ApplicationUser("a@b.c", isTeamLeader: true),
                await this.Seed.ApplicationUser("x@y.z", isTeamLeader: true)
            };

            this.Seed.Reservation(teamLeaderUsers.First(), date.PlusDays(1), 0);

            // Act
            using (var scope = this.CreateScope())
            {
                await CreateReservationReminder(scope).Run();
            }

            // Assert
            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                Assert.Empty(context.EmailQueueItems);
            }
        }

        [Theory]
        [InlineData(22, 10, 23, 10)]
        [InlineData(23, 10, 26, 9)]
        public void Test_GetNextRunTime(int currentDay, int currentHour, int expectedDay, int expectedHour)
        {
            // Arrange
            var currentInstant = currentDay.March(2018).At(currentHour, 00, 00).Utc();
            this.SetClock(currentInstant);

            using (var scope = this.CreateScope())
            {
                // Act
                var result = CreateReservationReminder(scope).GetNextRunTime(currentInstant);

                // Assert
                var expected = expectedDay.March(2018).At(expectedHour, 00, 00).Utc();

                Assert.Equal(expected, result);
            }
        }

        private static ReservationReminder CreateReservationReminder(IServiceScope scope) =>
            scope.ServiceProvider
                .GetRequiredService<IEnumerable<IScheduledTask>>()
                .OfType<ReservationReminder>()
                .Single();
    }
}