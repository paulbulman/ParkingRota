namespace ParkingRota.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Business.Model;

    public class ScheduledTaskRepository : IScheduledTaskRepository
    {
        private readonly IApplicationDbContext context;
        private readonly IMapper mapper;

        public ScheduledTaskRepository(IApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public IReadOnlyList<Business.Model.ScheduledTask> GetScheduledTasks() =>
            this.context.ScheduledTasks
                .ToArray()
                .Select(this.mapper.Map<Business.Model.ScheduledTask>)
                .ToArray();

        public void UpdateScheduledTask(Business.Model.ScheduledTask updated)
        {
            var existing = this.context.ScheduledTasks
                .Single(t => t.ScheduledTaskType == updated.ScheduledTaskType);

            existing.NextRunTime = updated.NextRunTime;

            this.context.SaveChanges();
        }
    }
}