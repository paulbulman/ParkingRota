namespace ParkingRota.Pages
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Business.Model;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public class OverrideRequestsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;

        public OverrideRequestsModel(UserManager<ApplicationUser> userManager) => this.userManager = userManager;

        public string SelectedUserId;

        public List<SelectListItem> Users;

        public void OnGet(string id)
        {
            this.SelectedUserId = id;

            this.Users = this.userManager.Users.Select(u => new SelectListItem(u.FullName, u.Id)).ToList();
        }
    }
}