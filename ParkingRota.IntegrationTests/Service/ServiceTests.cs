namespace ParkingRota.IntegrationTests.Service
{
    using System;
    using System.Threading.Tasks;
    using ParkingRota.Service;
    using Xunit;

    public class ServiceTests
    {
        [Fact]
        public async Task Test_RunTasks()
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CODEBUILD_BUILD_ID")))
            {
                return;
            }

            var program = new Service();

            await program.RunTasks();
        }
    }
}