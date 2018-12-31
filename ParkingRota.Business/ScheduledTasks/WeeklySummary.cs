﻿namespace ParkingRota.Business.ScheduledTasks
{
    using System.Linq;
    using System.Threading.Tasks;
    using Model;
    using NodaTime;

    public class WeeklySummary : IScheduledTask
    {
        private readonly IAllocationRepository allocationRepository;
        private readonly IDateCalculator dateCalculator;
        private readonly IEmailRepository emailRepository;
        private readonly IRequestRepository requestRepository;

        public WeeklySummary(
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

        public ScheduledTaskType ScheduledTaskType => ScheduledTaskType.WeeklySummary;

        public Task Run()
        {
            var summaryDates = this.dateCalculator.GetWeeklySummaryDates();

            var firstDate = summaryDates.First();
            var lastDate = summaryDates.Last();

            var allocations = this.allocationRepository.GetAllocations(firstDate, lastDate);
            var requests = this.requestRepository.GetRequests(firstDate, lastDate);

            foreach (var applicationUser in requests.Select(r => r.ApplicationUser).Distinct())
            {
                this.emailRepository.AddToQueue(new Emails.WeeklySummary(applicationUser.Email, allocations, requests));
            }

            return Task.CompletedTask;
        }

        public Instant GetNextRunTime(Instant currentInstant) =>
            currentInstant
                .InZone(this.dateCalculator.TimeZone)
                .Date
                .Next(IsoDayOfWeek.Thursday)
                .AtMidnight()
                .InZoneStrictly(this.dateCalculator.TimeZone)
                .ToInstant();
    }
}