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
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CODEBUILD_BUILD_ID")) ||
                !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BUILD_ID")))
            {
                return;
            }

            var program = new Service();

            program.RunTasks();
        }
    }
}