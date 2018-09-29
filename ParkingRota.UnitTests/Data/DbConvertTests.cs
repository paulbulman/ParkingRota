// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
namespace ParkingRota.UnitTests.Data
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using ParkingRota.Data;
    using Xunit;

    public static class DbConvertTests
    {
        public static IEnumerable<object[]> Utc_ToDb_Data()
        {
            // Winter
            yield return new object[]
            {
                CreateInstantFromUtc(2018, 01, 05, 13, 54, 37, 22),
                new DateTime(2018, 01, 05, 13, 54, 37, 22, DateTimeKind.Utc)
            };

            // Summer
            yield return new object[]
            {
                CreateInstantFromUtc(2078, 08, 18, 04, 56, 15, 03),
                new DateTime(2078, 08, 18, 04, 56, 15, 03, DateTimeKind.Utc)
            };
        }

        [Theory, MemberData(nameof(Utc_ToDb_Data))]
        public static void Test_Utc_ToDb(Instant codeValue, DateTime expectedDbValue)
        {
            var actualDbValue = DbConvert.Instant.ToDb(codeValue);

            CompareDateTimes(actualDbValue, expectedDbValue);
        }

        public static IEnumerable<object[]> Utc_FromDb_Data()
        {

            // Winter, UTC
            yield return new object[]
            {
                new DateTime(2007, 12, 22, 12, 09, 55, 41, DateTimeKind.Utc),
                CreateInstantFromUtc(2007, 12, 22, 12, 09, 55, 41)
            };

            // Winter, local
            yield return new object[]
            {
                new DateTime(2007, 12, 22, 12, 09, 55, 41, DateTimeKind.Local),
                CreateInstantFromUtc(2007, 12, 22, 12, 09, 55, 41)
            };

            // Winter, unspecified
            yield return new object[]
            {
                new DateTime(2007, 12, 22, 12, 09, 55, 41, DateTimeKind.Unspecified),
                CreateInstantFromUtc(2007, 12, 22, 12, 09, 55, 41)
            };

            // Summer, UTC
            yield return new object[]
            {
                new DateTime(2046, 06, 23, 10, 53, 05, 38, DateTimeKind.Utc),
                CreateInstantFromUtc(2046, 06, 23, 10, 53, 05, 38)
            };

            // Summer, local
            yield return new object[]
            {
                new DateTime(2046, 06, 23, 10, 53, 05, 38, DateTimeKind.Local),
                CreateInstantFromUtc(2046, 06, 23, 10, 53, 05, 38)
            };

            // Summer, unspecified
            yield return new object[]
            {
                new DateTime(2046, 06, 23, 10, 53, 05, 38, DateTimeKind.Unspecified),
                CreateInstantFromUtc(2046, 06, 23, 10, 53, 05, 38)
            };

        }

        [Theory, MemberData(nameof(Utc_FromDb_Data))]
        public static void Test_Utc_FromDb(DateTime dbValue, Instant expectedCodeValue)
        {
            var actualCodeValue = DbConvert.Instant.FromDb(dbValue);

            Assert.Equal(expectedCodeValue, actualCodeValue);
        }

        public static IEnumerable<object[]> LocalDate_ToDb_Data()
        {
            // Winter
            yield return new object[]
            {
                new LocalDate(2090, 01, 22),
                new DateTime(2090, 01, 22, 00, 00, 00, 00, DateTimeKind.Unspecified)
            };

            // Summer
            yield return new object[]
            {
                new LocalDate(2079, 06, 24),
                new DateTime(2079, 06, 24, 00, 00, 00, 00, DateTimeKind.Unspecified)
            };
        }

        [Theory, MemberData(nameof(LocalDate_ToDb_Data))]
        public static void Test_LocalDate_ToDb(LocalDate codeValue, DateTime expectedDbValue)
        {
            var actualDbValue = DbConvert.LocalDate.ToDb(codeValue);

            CompareDateTimes(actualDbValue, expectedDbValue);
        }

        public static IEnumerable<object[]> LocalDate_FromDb_Data()
        {
            // Winter, UTC
            yield return new object[]
            {
                new DateTime(2095, 01, 02, 00, 00, 00, 00, DateTimeKind.Utc),
                new LocalDate(2095, 01, 02)
            };

            // Winter, local
            yield return new object[]
            {
                new DateTime(2095, 01, 02, 00, 00, 00, 00, DateTimeKind.Local),
                new LocalDate(2095, 01, 02)
            };

            // Winter, unspecified
            yield return new object[]
            {
                new DateTime(2095, 01, 02, 00, 00, 00, 00, DateTimeKind.Unspecified),
                new LocalDate(2095, 01, 02)
            };

            // Summer, UTC
            yield return new object[]
            {
                new DateTime(2040, 08, 25, 00, 00, 00, 00, DateTimeKind.Utc),
                new LocalDate(2040, 08, 25)
            };

            // Summer, local
            yield return new object[]
            {
                new DateTime(2040, 08, 25, 00, 00, 00, 00, DateTimeKind.Local),
                new LocalDate(2040, 08, 25)
            };

            // Summer, unspecified
            yield return new object[]
            {
                new DateTime(2040, 08, 25, 00, 00, 00, 00, DateTimeKind.Unspecified),
                new LocalDate(2040, 08, 25)
            };

            // With time portion
            yield return new object[]
            {
                new DateTime(2080, 01, 19, 23, 59, 59, 999),
                new LocalDate(2080, 01, 19)
            };
        }

        [Theory, MemberData(nameof(LocalDate_FromDb_Data))]
        public static void Test_LocalDate_FromDb(DateTime dbValue, LocalDate expectedCodeValue)
        {
            var actualCodeValue = DbConvert.LocalDate.FromDb(dbValue);

            Assert.Equal(actualCodeValue, expectedCodeValue);
        }

        private static void CompareDateTimes(DateTime actual, DateTime expected)
        {
            Assert.Equal(expected.Year, actual.Year);
            Assert.Equal(expected.Month, actual.Month);
            Assert.Equal(expected.Day, actual.Day);
            Assert.Equal(expected.Hour, actual.Hour);
            Assert.Equal(expected.Minute, actual.Minute);
            Assert.Equal(expected.Second, actual.Second);
            Assert.Equal(expected.Millisecond, actual.Millisecond);
            Assert.Equal(expected.Ticks, actual.Ticks);
            Assert.Equal(expected.Kind, actual.Kind);
        }

        // This method is required as Instant.FromUtc() only resolves to seconds, not milliseconds
        private static Instant CreateInstantFromUtc(
            int year, int month, int day, int hour, int minute, int second, int millisecond) =>
                new LocalDateTime(year, month, day, hour, minute, second, millisecond).InUtc().ToInstant();
    }
}