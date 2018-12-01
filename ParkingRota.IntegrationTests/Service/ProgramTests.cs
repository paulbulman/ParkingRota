namespace ParkingRota.IntegrationTests.Service
{
    using Amazon.Lambda.TestUtilities;
    using ParkingRota.Service;
    using Xunit;

    public class ProgramTests
    {
        [Fact]
        public void Test_RunTasks()
        {
            var program = new Program();
            var context = new TestLambdaContext();

            program.RunTasks(context);
        }
    }
}