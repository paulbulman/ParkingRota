namespace ParkingRota.Data
{
    using AutoMapper;
    using Business;
    using Business.EmailSenders;
    using Business.Model;
    using Business.ScheduledTasks;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime;

    public static class ServiceCollectionHelper
    {
        public static void RegisterServices(IServiceCollection services)
        {
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
            services.AddScoped<ISecretsManager, AwsSsmSecretsManager>();
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
        }
    }
}