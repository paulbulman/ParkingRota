namespace ParkingRota.DatabaseUpgrader
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using Microsoft.EntityFrameworkCore;

    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var connectionStringEnvironmentVariableName = args.Any() ? args[0] : "ParkingRotaAdminConnectionString";

            var connectionString = Environment.GetEnvironmentVariable(connectionStringEnvironmentVariableName);

            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("Connection string environment variable not set.");
                return;
            }

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
    }
}
