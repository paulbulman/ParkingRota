namespace ParkingRota
{
    using System;

    public static class Helpers
    {
        public static bool IsElasticBeanstalk()
        {
            var environmentVariable = Environment.GetEnvironmentVariable("IsElasticBeanstalk");

            return bool.TryParse(environmentVariable, out var value) && value;
        }
    }
}