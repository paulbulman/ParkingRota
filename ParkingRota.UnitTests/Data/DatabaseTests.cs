namespace ParkingRota.UnitTests.Data
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Moq;
    using NodaTime;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using ParkingRota.Data;

    public class DatabaseTests
    {
        private readonly ServiceProvider serviceProvider;

        private IReadOnlyList<LocalDate> bankHolidayDates;

        private Instant currentInstant;

        protected DatabaseTests()
        {
            var databaseIdentifier = Guid.NewGuid();

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(databaseIdentifier.ToString()));

            serviceCollection.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            ServiceCollectionHelper.RegisterServices(serviceCollection);

            var mockBankHolidayFetcher = new Mock<IBankHolidayFetcher>(MockBehavior.Strict);
            mockBankHolidayFetcher
                .Setup(f => f.Fetch())
                .Returns(() => Task.FromResult(this.bankHolidayDates));

            serviceCollection.Replace(
                new ServiceDescriptor(
                    typeof(IBankHolidayFetcher),
                    s => mockBankHolidayFetcher.Object,
                    ServiceLifetime.Scoped));

            var mockClock = new Mock<IClock>(MockBehavior.Strict);
            mockClock
                .Setup(c => c.GetCurrentInstant())
                .Returns(() => this.currentInstant);

            serviceCollection.Replace(
                new ServiceDescriptor(
                    typeof(IClock),
                    s => mockClock.Object,
                    ServiceLifetime.Singleton));

            this.serviceProvider = serviceCollection.BuildServiceProvider();

            this.Seed = new DatabaseSeeder(this.serviceProvider);
        }

        protected DatabaseSeeder Seed { get; }

        protected IServiceScope CreateScope() => this.serviceProvider
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope();

        protected void SetBankHolidayDates(IReadOnlyList<LocalDate> newBankHolidayDates) =>
            this.bankHolidayDates = newBankHolidayDates;

        protected void SetClock(Instant newInstant) =>
            this.currentInstant = newInstant;
    }
}