namespace ParkingRota.Calendar
{
    using System.Collections.Generic;

    public class Week
    {
        internal Week(IReadOnlyList<Day> days) => this.Days = days;

        public IReadOnlyList<Day> Days { get; }
    }
}