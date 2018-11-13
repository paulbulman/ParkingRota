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
    using NodaTime.Text;

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

        [TempData]
        public string StatusMessage { get; set; }

        public Calendar<bool> Calendar { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var activeDates = this.dateCalculator.GetActiveDates();

            var requests = this.requestRepository.GetRequests(activeDates.First(), activeDates.Last());

            var currentUser = await this.userManager.GetUserAsync(this.User);

            var calendarData = activeDates.ToDictionary(
                d => d,
                d => requests.Any(r => r.Date == d && r.ApplicationUser.Id == currentUser.Id));

            this.Calendar = Calendar<bool>.Create(calendarData);

            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync(IReadOnlyList<string> selectedDateStrings)
        {
            var currentUser = await this.userManager.GetUserAsync(this.User);

            var requests = selectedDateStrings
                .Select(text => LocalDatePattern.Iso.Parse(text))
                .Where(result => result.Success)
                .Select(result => new Request() { ApplicationUser = currentUser, Date = result.Value })
                .ToArray();

            this.requestRepository.UpdateRequests(currentUser, requests);

            this.StatusMessage = "Requests updated.";

            return this.RedirectToPage();
        }
    }
}