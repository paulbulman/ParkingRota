namespace ParkingRota.Business.ScheduledTasks
{
    using System.Threading.Tasks;
    using Model;
    using NodaTime;

    public interface IScheduledTask
    {
        ScheduledTaskType ScheduledTaskType { get; }

        Task Run();

        Instant GetNextRunTime(Instant currentInstant);
    }
}