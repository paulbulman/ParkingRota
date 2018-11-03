namespace ParkingRota.UnitTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using NodaTime;

    public static class TestHelpers
    {
        public static LocalDateTime At(this LocalDate localDate, int hour, int minute, int second) =>
            localDate.At(new LocalTime(hour, minute, second));

        public static Instant Utc(this LocalDateTime localDateTime) => localDateTime.InUtc().ToInstant();

        public static DbSet<T> ToDbSet<T>(this IEnumerable<T> items) where T : class
        {
            var data = items.AsQueryable();

            var mockSet = new Mock<DbSet<T>>(MockBehavior.Strict);

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            return mockSet.Object;
        }
    }
}