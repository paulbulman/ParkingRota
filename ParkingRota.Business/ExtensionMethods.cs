﻿namespace ParkingRota.Business
{
    using NodaTime;
    using NodaTime.Text;

    public static class ExtensionMethods
    {
        public static string ForDisplay(this LocalDate localDate) =>
            LocalDatePattern.CreateWithCurrentCulture("dd MMM").Format(localDate);
    }
}