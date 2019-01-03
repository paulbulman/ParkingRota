namespace ParkingRota.IntegrationTests.Service
{
    using System;
    using System.Threading.Tasks;
    using Amazon.Lambda.TestUtilities;
    using ParkingRota.Service;
    using Xunit;

    public class ProgramTests
    {
        [Fact]
        public async Task Test_RunTasks()
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CODEBUILD_BUILD_ID")))
            {
                return;
            }

            var program = new Program();
            var context = new TestLambdaContext();

            await program.RunTasks(context);
        }
    }
}