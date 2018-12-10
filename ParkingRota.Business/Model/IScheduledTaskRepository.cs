namespace ParkingRota.Business.Model
{
    using System.Collections.Generic;

    public interface IScheduledTaskRepository
    {
        IReadOnlyList<ScheduledTask> GetScheduledTasks();

        void UpdateScheduledTask(ScheduledTask updated);
    }
}