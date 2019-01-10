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

    public class SummaryModel : PageModel
    {
        private readonly IDateCalculator dateCalculator;
        private readonly IRequestRepository requestRepository;
        private readonly IAllocationRepository allocationRepository;
        private readonly UserManager<ApplicationUser> userManager;

        public SummaryModel(
            IDateCalculator dateCalculator,
            IRequestRepository requestRepository,
            IAllocationRepository allocationRepository,
            UserManager<ApplicationUser> userManager)
        {
            this.dateCalculator = dateCalculator;
            this.requestRepository = requestRepository;
            this.allocationRepository = allocationRepository;
            this.userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var activeDates = this.dateCalculator.GetActiveDates();

            var requests = this.requestRepository.GetRequests(activeDates.First(), activeDates.Last());
            var allocations = this.allocationRepository.GetAllocations(activeDates.First(), activeDates.Last());

            var currentUser = await this.userManager.GetUserAsync(this.User);

            var calendarData = new Dictionary<LocalDate, DisplayRequests>();

            foreach (var activeDate in activeDates)
            {
                var displayRequests = requests
                    .Where(r => r.Date == activeDate)
                    .ToLookup(r => allocations.Any(a => a.ApplicationUser.Id == r.ApplicationUser.Id && a.Date == r.Date));

                var allocatedRequests = displayRequests[true]
                    .OrderBy(r => r.ApplicationUser.LastName)
                    .Select(r => new DisplayRequest(r.ApplicationUser.FullName, r.ApplicationUser.Id == currentUser.Id))
                    .ToArray();

                var unallocatedRequests = displayRequests[false]
                    .OrderBy(r => r.ApplicationUser.LastName)
                    .Select(r => new DisplayRequest(r.ApplicationUser.FullName, r.ApplicationUser.Id == currentUser.Id))
                    .ToArray();

                calendarData.Add(activeDate, new DisplayRequests(allocatedRequests, unallocatedRequests));
            }

            this.Calendar = Calendar<DisplayRequests>.Create(calendarData);

            return this.Page();
        }

        public Calendar<DisplayRequests> Calendar { get; private set; }

        public class DisplayRequests
        {
            public DisplayRequests(
                IReadOnlyList<DisplayRequest> allocatedRequests,
                IReadOnlyList<DisplayRequest> unallocatedRequests)
            {
                this.AllocatedRequests = allocatedRequests;
                this.UnallocatedRequests = unallocatedRequests;
            }

            public IReadOnlyList<DisplayRequest> AllocatedRequests { get; }

            public IReadOnlyList<DisplayRequest> UnallocatedRequests { get; }
        }

        public class DisplayRequest
        {
            public DisplayRequest(string fullName, bool isCurrentUser)
            {
                this.FullName = fullName;
                this.IsCurrentUser = isCurrentUser;
            }

            public string FullName { get; }

            public bool IsCurrentUser { get; }
        }
    }
}
