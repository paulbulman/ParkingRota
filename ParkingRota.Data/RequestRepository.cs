namespace ParkingRota.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Business;
    using Business.Model;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;

    public class RequestRepository : IRequestRepository
    {
        private readonly IApplicationDbContext context;
        private readonly IDateCalculator dateCalculator;
        private readonly IMapper mapper;

        public RequestRepository(IApplicationDbContext context, IDateCalculator dateCalculator, IMapper mapper)
        {
            this.context = context;
            this.dateCalculator = dateCalculator;
            this.mapper = mapper;
        }

        public IReadOnlyList<Business.Model.Request> GetRequests(LocalDate firstDate, LocalDate lastDate)
        {
            var firstDbDate = DbConvert.LocalDate.ToDb(firstDate);
            var lastDbDate = DbConvert.LocalDate.ToDb(lastDate);

            return this.context.Requests
                .Include(r => r.ApplicationUser)
                .Where(r => r.DbDate >= firstDbDate && r.DbDate <= lastDbDate)
                .ToArray()
                .Select(this.mapper.Map<Business.Model.Request>)
                .ToArray();
        }

        public void UpdateRequests(ApplicationUser user, IReadOnlyList<Business.Model.RequestPostModel> requests)
        {
            var activeDates = this.dateCalculator.GetActiveDates();

            var firstDbDate = DbConvert.LocalDate.ToDb(activeDates.First());
            var lastDbDate = DbConvert.LocalDate.ToDb(activeDates.Last());

            var existingUserActiveRequests = this.context.Requests
                .Where(r => r.ApplicationUser == user && r.DbDate >= firstDbDate && r.DbDate <= lastDbDate)
                .ToArray();

            var requestsToRemove = existingUserActiveRequests
                .Where(existing => requests.All(r => existing.Date != r.Date));

            var requestsToAdd = requests
                .Where(r => existingUserActiveRequests.All(existing => existing.Date != r.Date))
                .Select(r => new Request { ApplicationUserId = user.Id, Date = r.Date, IsAllocated = false });

            this.context.Requests.RemoveRange(requestsToRemove);
            this.context.Requests.AddRange(requestsToAdd);

            var existingUserActiveAllocations = this.context.Allocations
                .Where(a => a.ApplicationUser == user && a.DbDate >= firstDbDate && a.DbDate <= lastDbDate)
                .ToArray();

            var allocationsToRemove = existingUserActiveAllocations
                .Where(existing => requests.All(r => existing.Date != r.Date));

            this.context.Allocations.RemoveRange(allocationsToRemove);

            this.context.SaveChanges();
        }
    }
}