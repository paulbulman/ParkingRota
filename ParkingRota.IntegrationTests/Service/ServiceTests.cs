namespace ParkingRota.IntegrationTests.Service
{
    using System;
    using ParkingRota.Service;
    using Xunit;

    public class ServiceTests
    {
        [Fact]
        public void Test_RunTasks()
        {
            var connectionString = Environment.GetEnvironmentVariable("ParkingRotaServiceTestConnectionString");

            if (!string.IsNullOrEmpty(connectionString))
            {
                new Service(connectionString).RunTasks();
            }
        }
    }
}