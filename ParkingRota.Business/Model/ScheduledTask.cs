namespace ParkingRota.Business.Model
{
    using NodaTime;

    public class ScheduledTask
    {
        public int Id { get; set; }

        public ScheduledTaskType ScheduledTaskType { get; set; }

        public Instant NextRunTime { get; set; }
    }
}