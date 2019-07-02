namespace ParkingRota.UnitTests.Business.ScheduledTasks
{
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using NodaTime;
    using NodaTime.Testing;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using ParkingRota.Business.ScheduledTasks;
    using Xunit;
    using DataScheduledTask = ParkingRota.Data.ScheduledTask;

    public class ScheduledTaskRunnerTests : DatabaseTests
    {
        [Fact]
        public async Task Test_Run()
        {
            // Arrange
            var currentInstant = 13.December(2018).At(10, 00, 01).Utc();

            var dueTime = currentInstant.Plus(-1.Seconds());
            var notDueTime = currentInstant.Plus(1.Seconds());

            var notDueTask = CreateTask(notDueTime, ScheduledTaskType.BankHolidayUpdater);
            var dueTask = CreateTask(dueTime, ScheduledTaskType.DailySummary);
            var otherDueTask = CreateTask(dueTime, ScheduledTaskType.RequestReminder);

            using (var context = this.CreateContext())
            {
                context.ScheduledTasks.AddRange(notDueTask, dueTask, otherDueTask);
                context.SaveChanges();
            }

            // Act
            using (var context = this.CreateContext())
            {
                var scheduledTasks = new IScheduledTask[]
                {
                    new BankHolidayUpdaterBuilder().WithCurrentInstant(currentInstant).Build(context),
                    new DailySummaryBuilder().WithCurrentInstant(currentInstant).Build(context),
                    new RequestReminderBuilder().WithCurrentInstant(currentInstant).Build(context)
                };

                var scheduledTaskRunner = new ScheduledTaskRunner(
                    new DateCalculator(new FakeClock(currentInstant), BankHolidayRepositoryTests.CreateRepository(context)),
                    ScheduledTaskRepositoryTests.CreateRepository(context),
                    scheduledTasks);

                await scheduledTaskRunner.Run();
            }

            // Assert
            using (var context = this.CreateContext())
            {
                var result = context.ScheduledTasks.ToArray();

                Assert.All(result, r => Assert.True(r.NextRunTime > currentInstant));

                var actualNotDueTaskNextRunTime = result
                    .Single(r => r.ScheduledTaskType == notDueTask.ScheduledTaskType)
                    .NextRunTime;

                Assert.Equal(notDueTime, actualNotDueTaskNextRunTime);
            }
        }

        private static DataScheduledTask CreateTask(Instant nextRunTime, ScheduledTaskType scheduledTaskType) =>
            new DataScheduledTask
            {
                NextRunTime = nextRunTime,
                ScheduledTaskType = scheduledTaskType
            };
    }
}