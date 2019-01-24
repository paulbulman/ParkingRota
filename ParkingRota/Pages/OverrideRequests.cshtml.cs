namespace ParkingRota.Pages
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Business.Model;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using NodaTime.Text;

    public class OverrideRequestsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IRequestRepository requestRepository;

        public OverrideRequestsModel(UserManager<ApplicationUser> userManager, IRequestRepository requestRepository)
        {
            this.userManager = userManager;
            this.requestRepository = requestRepository;
        }

        public string SelectedUserId { get; private set; }

        public List<SelectListItem> Users { get; private set; }

        [TempData]
        public string StatusMessage { get; set; }

        public void OnGet(string id)
        {
            this.SelectedUserId = id;

            this.Users = this.userManager.Users
                .OrderBy(u => u.LastName)
                .Select(u => new SelectListItem(u.FullName, u.Id))
                .ToList();
        }

        public IActionResult OnPost(string selectedUserId, IReadOnlyList<string> selectedDateStrings)
        {
            var currentUser = this.userManager.Users.SingleOrDefault(u => u.Id == selectedUserId);

            if (currentUser != null)
            {
                var requests = selectedDateStrings
                    .Select(text => LocalDatePattern.Iso.Parse(text))
                    .Where(result => result.Success)
                    .Select(result => new Request { ApplicationUser = currentUser, Date = result.Value })
                    .ToArray();

                this.requestRepository.UpdateRequests(currentUser, requests);
                
                this.StatusMessage = "Requests updated.";
            }

            return this.RedirectToPage();
        }
    }
}