namespace ParkingRota.UnitTests.Business.ScheduledTasks
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Data;
    using NodaTime;
    using NodaTime.Testing;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business;
    using ParkingRota.Business.Model;
    using ParkingRota.Business.ScheduledTasks;
    using ParkingRota.Data;

    public class ReservationReminderBuilder
    {
        private readonly Instant currentInstant;
        private readonly IList<ApplicationUser> teamLeaderUsers;

        public ReservationReminderBuilder() : this(29.June(2019).At(9, 15, 26).Utc(), new List<ApplicationUser>())
        {
        }

        private ReservationReminderBuilder(Instant currentInstant, IList<ApplicationUser> teamLeaderUsers)
        {
            this.currentInstant = currentInstant;
            this.teamLeaderUsers = teamLeaderUsers;
        }

        public ReservationReminderBuilder WithCurrentInstant(Instant newCurrentInstant) =>
            new ReservationReminderBuilder(newCurrentInstant, this.teamLeaderUsers);

        public ReservationReminderBuilder WithTeamLeaderUsers(params ApplicationUser[] newTeamLeaderUsers) =>
            new ReservationReminderBuilder(this.currentInstant, newTeamLeaderUsers);

        public ReservationReminder Build(IApplicationDbContext context)
        {
            var mockUserManager = TestHelpers.CreateMockUserManager();
            mockUserManager
                .Setup(u => u.GetUsersInRoleAsync(UserRole.TeamLeader))
                .Returns(Task.FromResult(this.teamLeaderUsers));

            return new ReservationReminder(
                new DateCalculator(
                    new FakeClock(this.currentInstant),
                    BankHolidayRepositoryTests.CreateRepository(context)),
                new EmailRepositoryBuilder().WithCurrentInstant(this.currentInstant).Build(context),
                new ReservationRepositoryBuilder().WithCurrentInstant(this.currentInstant).Build(context),
                mockUserManager.Object);
        }
    }
}