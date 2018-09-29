namespace ParkingRota.Data
{
    using System;
    using NodaTime;

    public interface IDbConverter<TDb, TCode>
    {
        TCode FromDb(TDb dbValue);

        TDb ToDb(TCode codeValue);
    }

    public static class DbConvert
    {
        public static readonly IDbConverter<DateTime, Instant> Instant = new InstantConverter();

        public static readonly IDbConverter<DateTime, LocalDate> LocalDate = new LocalDateConverter();

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

        private class LocalDateConverter : IDbConverter<DateTime, LocalDate>
        {
            public LocalDate FromDb(DateTime dbValue) =>
                new LocalDate(dbValue.Year, dbValue.Month, dbValue.Day);

            public DateTime ToDb(LocalDate codeValue) =>
                new DateTime(codeValue.Year, codeValue.Month, codeValue.Day, 0, 0, 0, DateTimeKind.Unspecified);
        }
    }
}