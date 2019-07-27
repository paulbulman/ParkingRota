namespace ParkingRota.Business.ScheduledTasks
{
    using System.Linq;
    using System.Threading.Tasks;
    using Model;
    using NodaTime;

    public class DailySummary : IScheduledTask
    {
        private readonly IAllocationRepository allocationRepository;
        private readonly IDateCalculator dateCalculator;
        private readonly IEmailRepository emailRepository;
        private readonly IRequestRepository requestRepository;

        public DailySummary(
            IAllocationRepository allocationRepository,
            IDateCalculator dateCalculator,
            IEmailRepository emailRepository,
            IRequestRepository requestRepository)
        {
            this.allocationRepository = allocationRepository;
            this.dateCalculator = dateCalculator;
            this.emailRepository = emailRepository;
            this.requestRepository = requestRepository;
        }

        public ScheduledTaskType ScheduledTaskType => ScheduledTaskType.DailySummary;

        public Task Run()
        {
            var nextWorkingDate = this.dateCalculator.GetNextWorkingDate();

            var allocations = this.allocationRepository.GetAllocations(nextWorkingDate, nextWorkingDate);
            var requests = this.requestRepository.GetRequests(nextWorkingDate, nextWorkingDate);

            foreach (var recipient in requests.Where(r => !r.ApplicationUser.IsVisitor).Select(r => r.ApplicationUser))
            {
                this.emailRepository.AddToQueue(new EmailTemplates.DailySummary(recipient, allocations, requests));
            }

            return Task.CompletedTask;
        }

        public Instant GetNextRunTime(Instant currentInstant) =>
            this.dateCalculator.GetNextWorkingDate()
                .At(new LocalTime(11, 0, 0))
                .InZoneStrictly(DateCalculator.LondonTimeZone)
                .ToInstant();
    }
}