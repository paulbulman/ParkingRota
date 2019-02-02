namespace ParkingRota.Business.ScheduledTasks
{
    using System.Linq;
    using System.Threading.Tasks;
    using Model;
    using NodaTime;

    public class BankHolidayUpdater : IScheduledTask
    {
        private readonly IBankHolidayFetcher bankHolidayFetcher;
        private readonly IBankHolidayRepository bankHolidayRepository;
        private readonly IDateCalculator dateCalculator;

        public BankHolidayUpdater(
            IBankHolidayFetcher bankHolidayFetcher,
            IBankHolidayRepository bankHolidayRepository,
            IDateCalculator dateCalculator)
        {
            this.bankHolidayFetcher = bankHolidayFetcher;
            this.bankHolidayRepository = bankHolidayRepository;
            this.dateCalculator = dateCalculator;
        }

        public ScheduledTaskType ScheduledTaskType => ScheduledTaskType.BankHolidayUpdater;

        public async Task Run()
        {
            var existingDates = this.bankHolidayRepository.GetBankHolidays().Select(b => b.Date);
            var allDates = await this.bankHolidayFetcher.Fetch();

            var newBankHolidays = allDates
                .Except(existingDates)
                .Select(d => new BankHoliday { Date = d })
                .ToArray();

            this.bankHolidayRepository.AddBankHolidays(newBankHolidays);
        }

        public Instant GetNextRunTime(Instant currentInstant) =>
            currentInstant
                .InZone(this.dateCalculator.TimeZone)
                .Date
                .Next(IsoDayOfWeek.Monday)
                .AtMidnight()
                .InZoneStrictly(this.dateCalculator.TimeZone)
                .ToInstant();
    }
}