namespace ParkingRota.Business.ScheduledTasks
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Model;
    using NodaTime;

    public class ScheduledTaskRunner
    {
        private readonly IDateCalculator dateCalculator;

        private readonly IScheduledTaskRepository scheduledTaskRepository;

        private readonly IReadOnlyList<IScheduledTask> scheduledTasks;

        public ScheduledTaskRunner(
            IDateCalculator dateCalculator,
            IScheduledTaskRepository scheduledTaskRepository,
            IReadOnlyList<IScheduledTask> scheduledTasks)
        {
            this.dateCalculator = dateCalculator;
            this.scheduledTaskRepository = scheduledTaskRepository;
            this.scheduledTasks = scheduledTasks;
        }

        public async Task Run()
        {
            var currentInstant = this.dateCalculator.CurrentInstant;

            var dueTasks = this.scheduledTaskRepository
                .GetScheduledTasks()
                .Where(t => IsDue(t, currentInstant))
                .Select(t => this.GetScheduledTask(t.ScheduledTaskType));

            foreach (var scheduledTask in dueTasks)
            {
                await scheduledTask.Run();

                var updatedDefinition = new ScheduledTask
                {
                    NextRunTime = scheduledTask.GetNextRunTime(currentInstant),
                    ScheduledTaskType = scheduledTask.ScheduledTaskType
                };

                this.scheduledTaskRepository.UpdateScheduledTask(updatedDefinition);
            }
        }

        private static bool IsDue(ScheduledTask scheduledTask, Instant currentInstant) =>
            scheduledTask.NextRunTime <= currentInstant;

        private IScheduledTask GetScheduledTask(ScheduledTaskType scheduledTaskType) =>
            this.scheduledTasks.Single(t => t.ScheduledTaskType == scheduledTaskType);
    }
}