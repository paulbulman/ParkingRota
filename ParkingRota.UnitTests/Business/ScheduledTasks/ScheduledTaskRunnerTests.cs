namespace ParkingRota.UnitTests.Business.ScheduledTasks
{
    using System.Threading.Tasks;
    using Moq;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using ParkingRota.Business.ScheduledTasks;
    using Xunit;

    public static class ScheduledTaskRunnerTests
    {
        [Fact]
        public static async Task Test_Run()
        {
            // Arrange
            var currentInstant = 13.December(2018).At(10, 00, 01).Utc();
            var nextRunTime = 14.December(2018).At(10, 00, 00).Utc();

            var mockDateCalculator = new Mock<IDateCalculator>(MockBehavior.Strict);
            mockDateCalculator
                .SetupGet(d => d.CurrentInstant)
                .Returns(currentInstant);

            var dueTask = new Mock<IScheduledTask>(MockBehavior.Strict);
            dueTask.SetupGet(t => t.ScheduledTaskType).Returns(ScheduledTaskType.RequestReminder);
            dueTask.Setup(t => t.Run()).Returns(Task.CompletedTask);
            dueTask.Setup(t => t.GetNextRunTime(currentInstant)).Returns(nextRunTime);

            var notDueTask = new Mock<IScheduledTask>(MockBehavior.Strict);
            notDueTask.SetupGet(t => t.ScheduledTaskType).Returns(ScheduledTaskType.ReservationReminder);

            var scheduledTasks = new[]
            {
                new ScheduledTask { NextRunTime = currentInstant.Plus(1.Seconds()), ScheduledTaskType = notDueTask.Object.ScheduledTaskType},
                new ScheduledTask { NextRunTime = currentInstant.Plus(-1.Seconds()), ScheduledTaskType = dueTask.Object.ScheduledTaskType},
            };

            var mockScheduledTaskRepository = new Mock<IScheduledTaskRepository>(MockBehavior.Strict);
            mockScheduledTaskRepository
                .Setup(r => r.GetScheduledTasks())
                .Returns(scheduledTasks);
            mockScheduledTaskRepository
                .Setup(r => r.UpdateScheduledTask(It.IsAny<ScheduledTask>()));

            // Act
            var scheduledTaskRunner = new ScheduledTaskRunner(
                mockDateCalculator.Object,
                mockScheduledTaskRepository.Object,
                new[] { dueTask.Object, notDueTask.Object });

            await scheduledTaskRunner.Run();

            // Assert
            dueTask.Verify(t => t.Run(), Times.Once);
            mockScheduledTaskRepository.Verify(
                r => r.UpdateScheduledTask(It.Is<ScheduledTask>(t =>
                    t.ScheduledTaskType == dueTask.Object.ScheduledTaskType &&
                    t.NextRunTime == nextRunTime)),
                Times.Once);
        }
    }
}