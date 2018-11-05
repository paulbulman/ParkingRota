namespace ParkingRota.Business
{
    using System.Collections.Generic;
    using System.Linq;
    using Model;
    using NodaTime;

    public interface IDateCalculator
    {
        IReadOnlyList<LocalDate> GetActiveDates();
    }

    public class DateCalculator : IDateCalculator
    {
        private readonly IBankHolidayRepository bankHolidayRepository;

        private readonly Instant currentInstant;

        public DateCalculator(IClock clock, IBankHolidayRepository bankHolidayRepository)
        {
            this.bankHolidayRepository = bankHolidayRepository;
            this.currentInstant = clock.GetCurrentInstant();
        }

        public IReadOnlyList<LocalDate> GetActiveDates()
        {
            var currentDate = this.GetCurrentDate();

            var firstDayOfThisMonth = new LocalDate(currentDate.Year, currentDate.Month, 1);
            var firstDayOfSubsequentMonth = firstDayOfThisMonth.PlusMonths(2);
            var lastDayOfNextMonth = firstDayOfSubsequentMonth.PlusDays(-1);

            return this.DatesBetween(currentDate, lastDayOfNextMonth);
        }

        private LocalDate GetCurrentDate() => this.GetCurrentTime().Date;

        private ZonedDateTime GetCurrentTime() =>
            this.currentInstant.InZone(DateTimeZoneProviders.Tzdb["Europe/London"]);

        private IReadOnlyList<LocalDate> DatesBetween(LocalDate firstDate, LocalDate lastDate) =>
            Enumerable.Range(0, Period.Between(firstDate, lastDate, PeriodUnits.Days).Days + 1)
                .Select(firstDate.PlusDays)
                .Where(this.IsWorkingDay)
                .ToArray();

        private bool IsWorkingDay(LocalDate date) =>
            date.DayOfWeek != IsoDayOfWeek.Saturday &&
            date.DayOfWeek != IsoDayOfWeek.Sunday &&
            this.bankHolidayRepository.BankHolidays.All(b => b.Date != date);
    }
}