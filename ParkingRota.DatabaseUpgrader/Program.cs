namespace ParkingRota.DatabaseUpgrader
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using CommandLine;
    using Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    internal static class Program
    {
        private class Options
        {
            private const string HelpText =
                "The name of an environment variable containing a connection string for the database to upgrade.";

            [Option('e', "environmentVariableName", Required = false, HelpText = HelpText)]
            public string EnvironmentVariableName { get; set; }
        }

        private static async Task Main(string[] args)
        {
            string environmentVariableName = null;
            string connectionString = null;

            Parser.Default
                .ParseArguments<Options>(args)
                .WithParsed(o => environmentVariableName = o.EnvironmentVariableName);

            if (!string.IsNullOrEmpty(environmentVariableName))
            {
                connectionString = Environment.GetEnvironmentVariable(environmentVariableName);
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = GetConfiguration().GetConnectionString("DefaultConnection");
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string not set.");
            }

            await UpgradeDatabase(connectionString);
        }

        private static async Task UpgradeDatabase(string connectionString)
        {
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString);

            using (var context = new ApplicationDbContext(dbContextOptionsBuilder.Options))
            {
                var pendingMigrationNames = (await context.Database.GetPendingMigrationsAsync()).ToArray();

                if (pendingMigrationNames.Any())
                {
                    Console.WriteLine("Applying the following pending database migrations:");

                    foreach (var pendingMigrationName in pendingMigrationNames)
                    {
                        Console.WriteLine($"- {pendingMigrationName}");
                    }

                    await context.Database.MigrateAsync();

                    Console.WriteLine();
                    Console.WriteLine("Database migration complete.");
                }
                else
                {
                    Console.WriteLine("Database already up-to-date.");
                }
            }
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
