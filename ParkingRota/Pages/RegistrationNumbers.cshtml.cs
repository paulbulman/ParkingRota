namespace ParkingRota.Pages
{
    using System.Collections.Generic;
    using System.Linq;
    using Business.Model;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    public class RegistrationNumbersModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;

        public RegistrationNumbersModel(UserManager<ApplicationUser> userManager) => this.userManager = userManager;

        public IReadOnlyList<ApplicationUser> ApplicationUsers { get; private set; }

        public void OnGet() =>
            this.ApplicationUsers = this.userManager.Users.OrderBy(u => u.CarRegistrationNumber).ToArray();
    }
}