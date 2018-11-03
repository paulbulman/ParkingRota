namespace ParkingRota.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Business.Model;

    public class BankHolidayRepository : IBankHolidayRepository
    {
        private readonly IApplicationDbContext context;
        private readonly IMapper mapper;

        public BankHolidayRepository(IApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public IReadOnlyList<Business.Model.BankHoliday> BankHolidays =>
            this.context.BankHolidays
                .Select(this.mapper.Map<Business.Model.BankHoliday>)
                .ToArray();
    }
}