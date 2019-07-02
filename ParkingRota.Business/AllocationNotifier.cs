namespace ParkingRota.Business
{
    using System.Collections.Generic;
    using System.Linq;
    using EmailTemplates;
    using Model;
    using NodaTime;

    public class AllocationNotifier
    {
        private readonly IDateCalculator dateCalculator;
        private readonly IEmailRepository emailRepository;
        private readonly IScheduledTaskRepository scheduledTaskRepository;

        public AllocationNotifier(
            IDateCalculator dateCalculator,
            IEmailRepository emailRepository,
            IScheduledTaskRepository scheduledTaskRepository)
        {
            this.dateCalculator = dateCalculator;
            this.emailRepository = emailRepository;
            this.scheduledTaskRepository = scheduledTaskRepository;
        }

        public void Notify(IReadOnlyList<Allocation> allocations)
        {
            var datesToExclude = new List<LocalDate>();

            var scheduledTasks = this.scheduledTaskRepository.GetScheduledTasks();

            if (this.SummaryTaskIsDue(scheduledTasks, ScheduledTaskType.DailySummary))
            {
                datesToExclude.Add(this.dateCalculator.GetNextWorkingDate());
            }

            if (this.SummaryTaskIsDue(scheduledTasks, ScheduledTaskType.WeeklySummary))
            {
                datesToExclude.AddRange(this.dateCalculator.GetWeeklySummaryDates());
            }

            foreach (var allocation in allocations.Where(a => !datesToExclude.Contains(a.Date) && !a.ApplicationUser.IsVisitor))
            {
                this.emailRepository.AddToQueue(new SingleAllocation(allocation));
            }
        }

        private bool SummaryTaskIsDue(IReadOnlyList<ScheduledTask> scheduledTasks, ScheduledTaskType summaryTaskType)
        {
            var summaryTask = scheduledTasks.Single(t => t.ScheduledTaskType == summaryTaskType);

            return summaryTask.NextRunTime <= this.dateCalculator.CurrentInstant;
        }
    }
}