namespace ParkingRota.Service
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Business;
    using Business.EmailSenders;
    using Business.Model;
    using Business.ScheduledTasks;
    using Data;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NodaTime;

    public class TaskRunner : IDisposable
    {
        private readonly ServiceProvider serviceProvider;

        public TaskRunner(string connectionString) => this.serviceProvider = this.BuildServiceProvider(connectionString);

        public TaskRunner() => this.serviceProvider = this.BuildServiceProvider(null);

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

        private ServiceProvider BuildServiceProvider(string connectionString)
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

            services.AddSingleton<IClock>(SystemClock.Instance);

            services.AddScoped<AllocationCreator>();
            services.AddScoped<AllocationNotifier>();
            services.AddScoped<IAllocationRepository, AllocationRepository>();
            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
            services.AddHttpClient<IBankHolidayFetcher, BankHolidayFetcher>();
            services.AddScoped<IBankHolidayRepository, BankHolidayRepository>();
            services.AddScoped<IDateCalculator, DateCalculator>();
            services.AddScoped<EmailProcessor>();
            services.AddScoped<IEmailRepository, EmailRepository>();
            services.AddScoped<IEmailSender, LocalPapercutEmailSender>();
            services.AddScoped<IEmailSender, SendGridEmailSender>();
            services.AddScoped<IEmailSender, AwsSesEmailSender>();
            services.AddScoped<LastServiceRunTimeUpdater>();
            services.AddHttpClient<IPasswordBreachChecker, PasswordBreachChecker>();
            services.AddScoped<IRegistrationTokenRepository, RegistrationTokenRepository>();
            services.AddScoped<IRegistrationTokenValidator, RegistrationTokenValidator>();
            services.AddScoped<IRequestRepository, RequestRepository>();
            services.AddScoped<IRequestSorter, RequestSorter>();
            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<ISingleDayAllocationCreator, SingleDayAllocationCreator>();
            services.AddScoped<IScheduledTaskRepository, ScheduledTaskRepository>();
            services.AddScoped<ScheduledTaskRunner>();
            services.AddScoped<ISystemParameterListRepository, SystemParameterListRepository>();

            services.AddScoped<IScheduledTask, BankHolidayUpdater>();
            services.AddScoped<IScheduledTask, DailySummary>();
            services.AddScoped<IScheduledTask, RequestReminder>();
            services.AddScoped<IScheduledTask, ReservationReminder>();
            services.AddScoped<IScheduledTask, WeeklySummary>();

            services.AddSingleton<IMapper>(MapperBuilder.Build());

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