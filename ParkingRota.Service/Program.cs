namespace ParkingRota.Service
{
    using System;
    using Amazon.Lambda.Core;
    using AutoMapper;
    using Business;
    using Business.Model;
    using Business.ScheduledTasks;
    using Data;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime;

    public class Program
    {
        public async void RunTasks(ILambdaContext context)
        {
            var services = new ServiceCollection();

            var connectionString =
                Environment.GetEnvironmentVariable("ParkingRotaConnectionString") ??
                GetConfiguration().GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddSingleton<IClock>(SystemClock.Instance);

            services.AddScoped<AllocationCreator>();
            services.AddScoped<AllocationNotifier>();
            services.AddScoped<IAllocationRepository, AllocationRepository>();
            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
            services.AddScoped<IBankHolidayRepository, BankHolidayRepository>();
            services.AddScoped<IDateCalculator, DateCalculator>();
            services.AddScoped<EmailProcessor>();
            services.AddScoped<IEmailRepository, EmailRepository>();
            services.AddScoped<IEmailSender, EmailSender>();
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
            
            services.AddScoped<IScheduledTask, DailySummary>();
            services.AddScoped<IScheduledTask, RequestReminder>();
            services.AddScoped<IScheduledTask, ReservationReminder>();
            services.AddScoped<IScheduledTask, WeeklySummary>();

            var mapperConfiguration = new MapperConfiguration(c =>
            {
                c.CreateMap<Data.Allocation, Business.Model.Allocation>();
                c.CreateMap<Data.BankHoliday, Business.Model.BankHoliday>();
                c.CreateMap<Data.EmailQueueItem, Business.Model.EmailQueueItem>();
                c.CreateMap<Data.RegistrationToken, Business.Model.RegistrationToken>();
                c.CreateMap<Data.Request, Business.Model.Request>();
                c.CreateMap<Data.Reservation, Business.Model.Reservation>();
                c.CreateMap<Data.ScheduledTask, Business.Model.ScheduledTask>();
                c.CreateMap<Data.SystemParameterList, Business.Model.SystemParameterList>();
            });

            services.AddSingleton<IMapper>(new Mapper(mapperConfiguration));

            var serviceScopeFactory = services
                .BuildServiceProvider()
                .GetRequiredService<IServiceScopeFactory>();

            using (var scope = serviceScopeFactory.CreateScope())
            {
                var allocationCreator = scope.ServiceProvider.GetRequiredService<AllocationCreator>();
                var newAllocations = allocationCreator.Create();

                var allocationNotifier = scope.ServiceProvider.GetRequiredService<AllocationNotifier>();
                allocationNotifier.Notify(newAllocations);

                var scheduledTaskRunner = scope.ServiceProvider.GetRequiredService<ScheduledTaskRunner>();
                await scheduledTaskRunner.Run();

                var emailProcessor = scope.ServiceProvider.GetRequiredService<EmailProcessor>();
                await emailProcessor.SendPending();
            }
        }

        private static IConfiguration GetConfiguration() =>
            new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("AppSettings.json").Build();
    }
}
