﻿namespace ParkingRota.UnitTests.Business
{
    using NodaTime;
    using ParkingRota.Business;
    using Xunit;

    public static class ExtensionMethodsTests
    {
        [Theory]
        [InlineData(2018, 11, 7, "07 Nov")]
        [InlineData(2019, 3, 2, "02 Mar")]
        public static void Test_LocalDate_ForDisplay(int year, int month, int day, string expectedText)
        {
            var localDate = new LocalDate(year, month, day);

            Assert.Equal(expectedText, localDate.ForDisplay());
        }
    }
}