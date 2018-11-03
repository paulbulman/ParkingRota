namespace ParkingRota.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Business.Model;
    using NodaTime;

    public class RequestRepository : IRequestRepository
    {
        private readonly IApplicationDbContext context;
        private readonly IMapper mapper;

        public RequestRepository(IApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public IReadOnlyList<Business.Model.Request> GetRequests(LocalDate firstDate, LocalDate lastDate)
        {
            var firstDbDate = DbConvert.LocalDate.ToDb(firstDate);
            var lastDbDate = DbConvert.LocalDate.ToDb(lastDate);

            return this.context.Requests
                .Where(r => r.DbDate >= firstDbDate && r.DbDate <= lastDbDate)
                .ToArray()
                .Select(this.mapper.Map<Business.Model.Request>)
                .ToArray();
        }
    }
}