namespace ParkingRota.Data
{
    using System;
    using NodaTime;

    public interface IDbConverter<TDb, TCode>
    {
        TCode FromDb(TDb dbValue);

        TDb ToDb(TCode codeValue);
    }

    public class DbConvert
    {
        public static readonly IDbConverter<DateTime, Instant> Instant = new InstantConverter();

        private class InstantConverter : IDbConverter<DateTime, Instant>
        {
            public Instant FromDb(DateTime dbValue)
            {
                var utcValue = new LocalDateTime(
                    dbValue.Year,
                    dbValue.Month,
                    dbValue.Day,
                    dbValue.Hour,
                    dbValue.Minute,
                    dbValue.Second,
                    dbValue.Millisecond);

                return utcValue.InUtc().ToInstant();
            }

            public DateTime ToDb(Instant codeValue)
            {
                var utcValue = codeValue.InUtc();

                return new DateTime(
                    utcValue.Year,
                    utcValue.Month,
                    utcValue.Day,
                    utcValue.Hour,
                    utcValue.Minute,
                    utcValue.Second,
                    utcValue.Millisecond,
                    DateTimeKind.Utc);
            }
        }
    }
}