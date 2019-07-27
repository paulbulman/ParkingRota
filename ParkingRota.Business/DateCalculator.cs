namespace ParkingRota.Business
{
    using System.Collections.Generic;
    using System.Linq;
    using Model;
    using NodaTime;

    public interface IDateCalculator
    {
        Instant CurrentInstant { get; }

        IReadOnlyList<LocalDate> GetActiveDates();

        IReadOnlyList<LocalDate> GetShortLeadTimeAllocationDates();

        IReadOnlyList<LocalDate> GetLongLeadTimeAllocationDates();

        IReadOnlyList<LocalDate> GetWeeklySummaryDates();

        LocalDate GetCurrentDate();

        LocalDate GetNextWorkingDate();

        IReadOnlyList<LocalDate> GetUpcomingLongLeadTimeAllocationDates();
    }

    public class DateCalculator : IDateCalculator
    {
        public static readonly DateTimeZone LondonTimeZone = DateTimeZoneProviders.Tzdb["Europe/London"];

        private readonly IBankHolidayRepository bankHolidayRepository;

        public DateCalculator(IClock clock, IBankHolidayRepository bankHolidayRepository)
        {
            this.bankHolidayRepository = bankHolidayRepository;
            this.CurrentInstant = clock.GetCurrentInstant();
        }

        public Instant CurrentInstant { get; }

        public IReadOnlyList<LocalDate> GetActiveDates()
        {
            var currentDate = this.GetCurrentDate();

            var firstDayOfThisMonth = new LocalDate(currentDate.Year, currentDate.Month, 1);
            var firstDayOfSubsequentMonth = firstDayOfThisMonth.PlusMonths(2);
            var lastDayOfNextMonth = firstDayOfSubsequentMonth.PlusDays(-1);

            return this.DatesBetween(currentDate, lastDayOfNextMonth);
        }

        public IReadOnlyList<LocalDate> GetShortLeadTimeAllocationDates() =>
            this.GetShortLeadTimeAllocationDates(this.GetCurrentTime());

        public IReadOnlyList<LocalDate> GetLongLeadTimeAllocationDates() =>
            this.GetLongLeadTimeAllocationDates(this.GetCurrentTime());

        public IReadOnlyList<LocalDate> GetWeeklySummaryDates()
        {
            var lastDate = GetLastLongLeadTimeAllocationDate(this.GetCurrentDate());
            var firstDate = lastDate.PlusDays(-4);

            return this.DatesBetween(firstDate, lastDate);
        }

        private IReadOnlyList<LocalDate> GetShortLeadTimeAllocationDates(ZonedDateTime currentTime)
        {
            var firstDate = this.GetNextWorkingDayIncluding(currentTime.Date);

            var lastDate = firstDate == currentTime.Date && currentTime.Hour >= 11 ?
                this.GetNextWorkingDayStrictlyAfter(firstDate) :
                firstDate;

            return new[] { firstDate, lastDate }.Distinct().ToList();
        }

        private IReadOnlyList<LocalDate> GetLongLeadTimeAllocationDates(ZonedDateTime currentTime)
        {
            var lastShortLeadTimeAllocationDate = this.GetShortLeadTimeAllocationDates(currentTime).Last();

            var firstDate = this.GetNextWorkingDayStrictlyAfter(lastShortLeadTimeAllocationDate);

            var lastDate = GetLastLongLeadTimeAllocationDate(currentTime.Date);

            return this.DatesBetween(firstDate, lastDate);
        }

        public LocalDate GetCurrentDate() => this.GetCurrentTime().Date;

        public LocalDate GetNextWorkingDate() => this.GetNextWorkingDayStrictlyAfter(this.GetCurrentDate());

        public IReadOnlyList<LocalDate> GetUpcomingLongLeadTimeAllocationDates()
        {
            var todayStart = this.GetCurrentTime().Date.AtStartOfDayInZone(LondonTimeZone);
            var tomorrowStart = this.GetCurrentTime().Date.PlusDays(1).AtStartOfDayInZone(LondonTimeZone);

            return this.GetLongLeadTimeAllocationDates(tomorrowStart)
                .Except(this.GetLongLeadTimeAllocationDates(todayStart))
                .ToArray();
        }

        private ZonedDateTime GetCurrentTime() => this.CurrentInstant.InZone(LondonTimeZone);

        private LocalDate GetNextWorkingDayStrictlyAfter(LocalDate localDate) =>
            this.GetNextWorkingDayIncluding(localDate.PlusDays(1));

        private LocalDate GetNextWorkingDayIncluding(LocalDate localDate)
        {
            while (!this.IsWorkingDay(localDate))
            {
                localDate = localDate.PlusDays(1);
            }

            return localDate;
        }

        private IReadOnlyList<LocalDate> DatesBetween(LocalDate firstDate, LocalDate lastDate) =>
            Enumerable.Range(0, Period.Between(firstDate, lastDate, PeriodUnits.Days).Days + 1)
                .Select(firstDate.PlusDays)
                .Where(this.IsWorkingDay)
                .ToArray();

        private bool IsWorkingDay(LocalDate date) =>
            date.DayOfWeek != IsoDayOfWeek.Saturday &&
            date.DayOfWeek != IsoDayOfWeek.Sunday &&
            this.bankHolidayRepository.GetBankHolidays().All(b => b.Date != date);

        private static LocalDate GetLastLongLeadTimeAllocationDate(LocalDate localDate) =>
            localDate.Next(IsoDayOfWeek.Thursday).PlusWeeks(1).PlusDays(1);
    }
}