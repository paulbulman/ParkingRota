namespace ParkingRota.UnitTests.Business.ScheduledTasks
{
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using Xunit;
    using DataReservation = ParkingRota.Data.Reservation;

    public class ReservationReminderTests : DatabaseTests
    {
        [Fact]
        public void Test_ScheduledTaskType()
        {
            using (var context = this.CreateContext())
            {
                var result = new ReservationReminderBuilder()
                    .Build(context)
                    .ScheduledTaskType;

                Assert.Equal(ScheduledTaskType.ReservationReminder, result);
            }
        }

        [Fact]
        public async Task Test_Run_NoReservationsEntered()
        {
            // Arrange
            var date = 13.December(2018);

            var teamLeaderUsers = new[]
            {
                new ApplicationUser { Email = "a@b.c" },
                new ApplicationUser { Email = "x@y.z" }
            };

            // Act
            using (var context = this.CreateContext())
            {
                await new ReservationReminderBuilder()
                    .WithCurrentInstant(date.At(10, 0, 0).Utc())
                    .WithTeamLeaderUsers(teamLeaderUsers)
                    .Build(context)
                    .Run();
            }

            // Assert
            using (var context = this.CreateContext())
            {
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

            using (var context = this.CreateContext())
            {
                context.Reservations.Add(new DataReservation { Date = date.PlusDays(1) });
            }

            // Act
            using (var context = this.CreateContext())
            {
                await new ReservationReminderBuilder()
                    .WithCurrentInstant(date.At(10, 0, 0).Utc())
                    .Build(context)
                    .Run();
            }

            // Assert
            using (var context = this.CreateContext())
            {
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

            using (var context = this.CreateContext())
            {
                // Act
                var result = new ReservationReminderBuilder()
                    .WithCurrentInstant(currentInstant)
                    .Build(context)
                    .GetNextRunTime(currentInstant);

                // Assert
                var expected = expectedDay.March(2018).At(expectedHour, 00, 00).Utc();

                Assert.Equal(expected, result);
            }
        }
    }
}