namespace ParkingRota.UnitTests.Business.ScheduledTasks
{
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.Model;
    using ParkingRota.Business.ScheduledTasks;
    using ParkingRota.Data;
    using Xunit;

    public class ScheduledTaskRunnerTests : DatabaseTests
    {
        [Fact]
        public async Task Test_Run()
        {
            // Arrange
            var currentInstant = 13.December(2018).At(10, 00, 01).Utc();
            this.SetClock(currentInstant);

            var dueTime = currentInstant.Plus(-1.Seconds());
            var notDueTime = currentInstant.Plus(1.Seconds());

            var notDueTask = this.Seed.ScheduledTask(notDueTime, ScheduledTaskType.BankHolidayUpdater);

            this.Seed.ScheduledTask(dueTime, ScheduledTaskType.DailySummary);
            this.Seed.ScheduledTask(dueTime, ScheduledTaskType.RequestReminder);

            // Act
            using (var scope = this.CreateScope())
            {
                await scope.ServiceProvider.GetRequiredService<ScheduledTaskRunner>().Run();
            }

            // Assert
            using (var scope = this.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var result = context.ScheduledTasks.ToArray();

                Assert.All(result, r => Assert.True(r.NextRunTime > currentInstant));

                var actualNotDueTaskNextRunTime = result
                    .Single(r => r.ScheduledTaskType == notDueTask.ScheduledTaskType)
                    .NextRunTime;

                Assert.Equal(notDueTime, actualNotDueTaskNextRunTime);
            }
        }
    }
}