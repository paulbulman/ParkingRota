namespace ParkingRota.Pages
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Business;
    using Business.Model;
    using Calendar;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using NodaTime;

    public class EditRequestsModel : PageModel
    {
        private readonly IDateCalculator dateCalculator;
        private readonly IRequestRepository requestRepository;
        private readonly UserManager<ApplicationUser> userManager;

        public EditRequestsModel(
            IDateCalculator dateCalculator,
            IRequestRepository requestRepository,
            UserManager<ApplicationUser> userManager)
        {
            this.dateCalculator = dateCalculator;
            this.requestRepository = requestRepository;
            this.userManager = userManager;
        }

        public Calendar Calendar { get; private set; }

        public IDictionary<LocalDate, bool> DisplayRequests { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            this.Calendar = Calendar.Create(this.dateCalculator);

            var currentUser = await this.userManager.GetUserAsync(this.User);

            var activeDates = this.dateCalculator.GetActiveDates();

            var requests = this.requestRepository.GetRequests(activeDates.First(), activeDates.Last());

            this.DisplayRequests = activeDates
                .ToDictionary(d => d, d => requests.Any(r => r.Date == d && r.ApplicationUser.Id == currentUser.Id));

            return this.Page();
        }
    }
}