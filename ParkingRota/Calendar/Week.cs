namespace ParkingRota.Calendar
{
    using System.Collections.Generic;

    public class Week<T>
    {
        internal Week(IReadOnlyList<Day<T>> days) => this.Days = days;

        public IReadOnlyList<Day<T>> Days { get; }
    }
}