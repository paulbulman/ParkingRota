namespace ParkingRota.UnitTests.Business.ScheduledTasks
{
    using System.Collections.Generic;
    using Data;
    using NodaTime;
    using NodaTime.Testing;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using ParkingRota.Business.ScheduledTasks;
    using ParkingRota.Data;

    public class RequestReminderBuilder
    {
        private readonly Instant currentInstant;
        private readonly IReadOnlyList<ApplicationUser> users;

        public RequestReminderBuilder() : this(29.June(2019).At(9, 15, 26).Utc(), new List<ApplicationUser>())
        {
        }

        private RequestReminderBuilder(Instant currentInstant, IReadOnlyList<ApplicationUser> users)
        {
            this.currentInstant = currentInstant;
            this.users = users;
        }

        public RequestReminderBuilder WithCurrentInstant(Instant newCurrentInstant) =>
            new RequestReminderBuilder(newCurrentInstant, this.users);

        public RequestReminderBuilder WithUsers(params ApplicationUser[] newUsers) =>
            new RequestReminderBuilder(this.currentInstant, newUsers);

        public RequestReminder Build(IApplicationDbContext context) =>
            new RequestReminder(
                new DateCalculator(new FakeClock(this.currentInstant), BankHolidayRepositoryTests.CreateRepository(context)),
                new EmailRepositoryBuilder().WithCurrentInstant(this.currentInstant).Build(context),
                new RequestRepositoryBuilder().WithCurrentInstant(this.currentInstant).Build(context),
                TestHelpers.CreateMockUserManager(this.users).Object);
    }
}