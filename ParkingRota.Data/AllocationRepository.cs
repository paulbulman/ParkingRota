namespace ParkingRota.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Business.Model;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;

    public class AllocationRepository : IAllocationRepository
    {
        private readonly IApplicationDbContext context;
        private readonly IMapper mapper;

        public AllocationRepository(IApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public IReadOnlyList<Business.Model.Allocation> GetAllocations(LocalDate firstDate, LocalDate lastDate)
        {
            var firstDbDate = DbConvert.LocalDate.ToDb(firstDate);
            var lastDbDate = DbConvert.LocalDate.ToDb(lastDate);

            return this.context.Allocations
                .Include(a => a.ApplicationUser)
                .Where(a => a.DbDate >= firstDbDate && a.DbDate <= lastDbDate)
                .ToArray()
                .Select(this.mapper.Map<Business.Model.Allocation>)
                .ToArray();
        }

        public void AddAllocations(IReadOnlyList<Business.Model.Allocation> allocations)
        {
            this.context.Allocations.AddRange(
                allocations.Select(a => new Allocation { ApplicationUserId = a.ApplicationUser.Id, Date = a.Date }));

            this.context.SaveChanges();
        }
    }
}