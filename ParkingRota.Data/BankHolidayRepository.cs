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

        private readonly IApplicationDbContext context;

        public BankHolidayRepository(IApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.bankHolidays = new Lazy<IReadOnlyList<ModelBankHoliday>>(CreateBankHolidays(context, mapper));
        }

        private static Func<IReadOnlyList<ModelBankHoliday>> CreateBankHolidays(IApplicationDbContext context, IMapper mapper) =>
            () => context.BankHolidays.ToArray().Select(mapper.Map<ModelBankHoliday>).ToArray();

        public void AddBankHolidays(IReadOnlyList<ModelBankHoliday> newBankHolidays)
        {
            this.context.BankHolidays.AddRange(newBankHolidays.Select(b => new BankHoliday { Date = b.Date }));
            this.context.SaveChanges();
        }

        public IReadOnlyList<ModelBankHoliday> GetBankHolidays() => this.bankHolidays.Value;
    }
}