namespace ParkingRota.Pages.Users
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Business.Model;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.EntityFrameworkCore;

    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;

        public IndexModel(UserManager<ApplicationUser> userManager) => this.userManager = userManager;

        public IList<ApplicationUser> Users { get; set; }

        public async Task OnGetAsync()
        {
            var currentUser = await this.userManager.GetUserAsync(this.User);

            this.Users = await this.userManager.Users
                .Where(u => u.Id != currentUser.Id)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();
        }
    }
}