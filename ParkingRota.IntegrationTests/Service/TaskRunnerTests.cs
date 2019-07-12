namespace ParkingRota.IntegrationTests.Service
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using ParkingRota.Service;
    using Xunit;
    using Xunit.Abstractions;

    public class TaskRunnerTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public TaskRunnerTests(ITestOutputHelper testOutputHelper) => this.testOutputHelper = testOutputHelper;

        [Fact]
        public async Task Test_RunTasks()
        {
            this.testOutputHelper.WriteLine("Reading connection string from environment variable ParkingRotaTestConnectionString");

            var connectionString = Environment.GetEnvironmentVariable("ParkingRotaTestConnectionString");

            if (string.IsNullOrEmpty(connectionString))
            {
                this.testOutputHelper.WriteLine("Reading connection string from DefaultConnection setting in appSettings");

                connectionString = GetConfiguration().GetConnectionString("DefaultConnection");
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                this.testOutputHelper.WriteLine("Connection string not set. Skipping test.");

                return;
            }

            var success = await new TaskRunner(connectionString).RunTasksAsync();

            Assert.True(success);
        }

        private static IConfiguration GetConfiguration()
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .Build();
        }
    }
}