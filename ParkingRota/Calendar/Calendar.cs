namespace ParkingRota.Calendar
{
    using System.Collections.Generic;
    using System.Linq;
    using Business;
    using NodaTime;

    public class Calendar
    {
        private Calendar(IReadOnlyList<Week> weeks) => this.Weeks = weeks;

        public IReadOnlyList<Week> Weeks { get; }

        public static Calendar Create(IDateCalculator dateCalculator)
        {
            var activeDates = dateCalculator.GetActiveDates();

            var weekStarts = activeDates
                .Select(d => d.Next(IsoDayOfWeek.Monday).PlusDays(-7))
                .Distinct()
                .OrderBy(d => d);

            var weeks = weekStarts
                .Select(d => CalculateWeek(d, activeDates))
                .ToArray();

            return new Calendar(weeks);
        }

        private static Week CalculateWeek(LocalDate firstDateOfWeek, IReadOnlyList<LocalDate> activeDates)
        {
            var dates = Enumerable
                .Range(0, 5)
                .Select(firstDateOfWeek.PlusDays);

            var days = dates
                .Select(d => new Day(d, isActive: activeDates.Contains(d)))
                .ToArray();

            return new Week(days);
        }
    }
}