namespace ParkingRota.IntegrationTests.Service
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using ParkingRota.Service;
    using Xunit;

    public class TaskRunnerTests
    {
        [Fact]
        public async Task Test_RunTasks()
        {
            var connectionString = Environment.GetEnvironmentVariable("ParkingRotaTestConnectionString");

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = GetConfiguration().GetConnectionString("DefaultConnection");
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                return;
            }

            await new TaskRunner(connectionString).RunTasksAsync();
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