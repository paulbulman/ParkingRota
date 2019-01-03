namespace ParkingRota.IntegrationTests.Service
{
    using System;
    using Amazon.Lambda.TestUtilities;
    using ParkingRota.Service;
    using Xunit;

    public class ProgramTests
    {
        [Fact]
        public void Test_RunTasks()
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CODEBUILD_BUILD_ID")))
            {
                return;
            }

            var program = new Program();
            var context = new TestLambdaContext();

            program.RunTasks(context);
        }
    }
}