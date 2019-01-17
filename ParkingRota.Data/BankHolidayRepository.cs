namespace ParkingRota.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Business.Model;
    using ModelBankHoliday = Business.Model.BankHoliday;

    public class BankHolidayRepository : IBankHolidayRepository
    {
        private readonly Lazy<IReadOnlyList<ModelBankHoliday>> bankHolidays;

        public BankHolidayRepository(IApplicationDbContext context, IMapper mapper) => 
            this.bankHolidays = new Lazy<IReadOnlyList<ModelBankHoliday>>(CreateBankHolidays(context, mapper));

        private static Func<IReadOnlyList<ModelBankHoliday>> CreateBankHolidays(IApplicationDbContext context, IMapper mapper) =>
            () => context.BankHolidays.ToArray().Select(mapper.Map<ModelBankHoliday>).ToArray();

        public IReadOnlyList<ModelBankHoliday> BankHolidays => this.bankHolidays.Value;
    }
}