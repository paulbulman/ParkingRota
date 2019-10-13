namespace ParkingRota.UnitTests.Data
{
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.Model;
    using ParkingRota.Data;
    using Xunit;
    using ModelScheduledTask = ParkingRota.Business.Model.ScheduledTask;

    public class ScheduledTaskRepositoryTests : DatabaseTests
    {
        [Fact]
        public void Test_GetScheduledTasks()
        {
            // Arrange
            var scheduledTasks = new[]
            {
                this.Seed.ScheduledTask(10.December(2018).At(20, 0, 0).Utc(), ScheduledTaskType.ReservationReminder),
                this.Seed.ScheduledTask(10.December(2018).At(21, 0, 0).Utc(), ScheduledTaskType.RequestReminder)
            };

            using (var scope = this.CreateScope())
            {
                // Act
                var result = scope.ServiceProvider
                    .GetRequiredService<IScheduledTaskRepository>()
                    .GetScheduledTasks();

                // Assert
                Assert.Equal(scheduledTasks.Length, result.Count);

                foreach (var scheduledTask in scheduledTasks)
                {
                    Assert.Single(result.Where(t =>
                        t.ScheduledTaskType == scheduledTask.ScheduledTaskType &&
                        t.NextRunTime == scheduledTask.NextRunTime));
                }
            }
        }

        [Fact]
        public void Test_UpdateScheduledTask()
        {
            // Arrange
            var reservationReminder =
                this.Seed.ScheduledTask(10.December(2018).At(20, 0, 0).Utc(), ScheduledTaskType.ReservationReminder);

            var requestReminder =
                this.Seed.ScheduledTask(10.December(2018).At(21, 0, 0).Utc(), ScheduledTaskType.RequestReminder);

            var scheduledTasks = new[] { reservationReminder, requestReminder };

            var updatedReservationReminder = new ModelScheduledTask
            {
                ScheduledTaskType = reservationReminder.ScheduledTaskType,
                NextRunTime = reservationReminder.NextRunTime.Plus(1.Minutes())
            };

            using (var scope = this.CreateScope())
            {
                // Act
                scope.ServiceProvider
                    .GetRequiredService<IScheduledTaskRepository>()
                    .UpdateScheduledTask(updatedReservationReminder);
            }

            // Assert
            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var result = context.ScheduledTasks.ToArray();

                Assert.Equal(scheduledTasks.Length, result.Length);

                Assert.Single(result.Where(t =>
                    t.ScheduledTaskType == updatedReservationReminder.ScheduledTaskType &&
                    t.NextRunTime == updatedReservationReminder.NextRunTime));

                Assert.Single(result.Where(t =>
                    t.ScheduledTaskType == requestReminder.ScheduledTaskType &&
                    t.NextRunTime == requestReminder.NextRunTime));
            }
        }
    }
}