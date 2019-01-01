namespace ParkingRota.Business.ScheduledTasks
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;
    using Model;
    using NodaTime;

    public class RequestReminder : IScheduledTask
    {
        private readonly IDateCalculator dateCalculator;
        private readonly IEmailRepository emailRepository;
        private readonly IRequestRepository requestRepository;
        private readonly UserManager<ApplicationUser> userManager;

        public RequestReminder(
            IDateCalculator dateCalculator,
            IEmailRepository emailRepository,
            IRequestRepository requestRepository,
            UserManager<ApplicationUser> userManager)
        {
            this.dateCalculator = dateCalculator;
            this.emailRepository = emailRepository;
            this.requestRepository = requestRepository;
            this.userManager = userManager;
        }

        public ScheduledTaskType ScheduledTaskType => ScheduledTaskType.RequestReminder;

        public Task Run()
        {
            var upcomingLongLeadTimeAllocationDates = this.dateCalculator.GetUpcomingLongLeadTimeAllocationDates();

            if (!upcomingLongLeadTimeAllocationDates.Any())
            {
                return Task.CompletedTask;
            }

            var firstDate = this.dateCalculator.GetCurrentDate().PlusDays(-30);
            var lastDate = upcomingLongLeadTimeAllocationDates.Last();

            var requests = this.requestRepository.GetRequests(firstDate, lastDate);

            var users = this.userManager.Users.ToArray();

            var activeUsersWithoutUpcomingRequests = users.Where(u =>
                requests.Any(r => r.ApplicationUser.Id == u.Id) &&
                !requests.Any(r => r.ApplicationUser.Id == u.Id && upcomingLongLeadTimeAllocationDates.Contains(r.Date)));

            foreach (var user in activeUsersWithoutUpcomingRequests)
            {
                this.emailRepository.AddToQueue(
                    new Emails.RequestReminder(
                        user.Email,
                        upcomingLongLeadTimeAllocationDates.First(),
                        upcomingLongLeadTimeAllocationDates.Last()));
            }

            return Task.CompletedTask;
        }

        public Instant GetNextRunTime(Instant currentInstant) =>
            currentInstant
                .InZone(this.dateCalculator.TimeZone)
                .Date
                .Next(IsoDayOfWeek.Wednesday)
                .AtMidnight()
                .InZoneStrictly(this.dateCalculator.TimeZone)
                .ToInstant();
    }
}