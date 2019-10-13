namespace ParkingRota.Service
{
    using System;
    using System.Threading.Tasks;
    using Business;
    using Business.Model;
    using Business.ScheduledTasks;
    using Data;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class TaskRunner : IDisposable
    {
        private readonly ServiceProvider serviceProvider;

        public TaskRunner(string connectionString) => this.serviceProvider = BuildServiceProvider(connectionString);

        public TaskRunner() => this.serviceProvider = BuildServiceProvider(null);

        public async Task<bool> RunTasksAsync()
        {
            using (var scope = this.serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Service>>();

                try
                {
                    logger.LogDebug("Running task loop...");

                    var allocationCreator = scope.ServiceProvider.GetRequiredService<AllocationCreator>();
                    var newAllocations = allocationCreator.Create();

                    var allocationNotifier = scope.ServiceProvider.GetRequiredService<AllocationNotifier>();
                    allocationNotifier.Notify(newAllocations);

                    var scheduledTaskRunner = scope.ServiceProvider.GetRequiredService<ScheduledTaskRunner>();
                    await scheduledTaskRunner.Run();

                    var emailProcessor = scope.ServiceProvider.GetRequiredService<EmailProcessor>();
                    await emailProcessor.SendPending();

                    var lastServiceRunTimeUpdater = scope.ServiceProvider.GetRequiredService<LastServiceRunTimeUpdater>();
                    lastServiceRunTimeUpdater.Update();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Exception whilst running task loop");
                    return false;
                }
            }

            return true;
        }

        private static ServiceProvider BuildServiceProvider(string connectionString)
        {
            var services = new ServiceCollection();

            var configuration = GetConfiguration();

            services.AddLogging(configure => configure
                .AddConsole()
                .AddConfiguration(configuration.GetSection("Logging")));

            var databaseConnectionString =
                connectionString ??
                Environment.GetEnvironmentVariable("ParkingRotaConnectionString") ??
                configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(databaseConnectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            ServiceCollectionHelper.RegisterServices(services);

            return services.BuildServiceProvider();
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

        public void Dispose() => this.serviceProvider.Dispose();
    }
}