namespace ParkingRota.Business.ScheduledTasks
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;
    using Model;

    public class ReservationsReminder
    {
        private readonly IDateCalculator dateCalculator;
        private readonly IEmailRepository emailRepository;
        private readonly IReservationRepository reservationRepository;
        private readonly UserManager<ApplicationUser> userManager;

        public ReservationsReminder(
            IDateCalculator dateCalculator,
            IEmailRepository emailRepository,
            IReservationRepository reservationRepository,
            UserManager<ApplicationUser> userManager)
        {
            this.dateCalculator = dateCalculator;
            this.emailRepository = emailRepository;
            this.reservationRepository = reservationRepository;
            this.userManager = userManager;
        }

        public async Task Run()
        {
            var nextWorkingDate = this.dateCalculator.GetNextWorkingDate();

            var reservations = this.reservationRepository.GetReservations(nextWorkingDate, nextWorkingDate);

            if (!reservations.Any())
            {
                var teamLeaderUsers = await this.userManager.GetUsersInRoleAsync(UserRole.TeamLeader);

                foreach (var teamLeaderUser in teamLeaderUsers)
                {
                    this.emailRepository.AddToQueue(
                        new Emails.ReservationsReminder(teamLeaderUser.Email, nextWorkingDate));
                }
            }
        }
    }
}