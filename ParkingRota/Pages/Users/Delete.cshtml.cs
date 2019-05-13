namespace ParkingRota.Pages.Users
{
    using System.Threading.Tasks;
    using Business.Model;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    public class DeleteModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;

        public DeleteModel(UserManager<ApplicationUser> userManager) => this.userManager = userManager;

        [BindProperty]
        public ApplicationUser ApplicationUser { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var userToDelete = await this.userManager.FindByIdAsync(id);
            var currentUser = await this.userManager.GetUserAsync(this.User);

            if (userToDelete == null || userToDelete == currentUser)
            {
                return this.NotFound();
            }

            this.ApplicationUser = userToDelete;

            return this.Page();

        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            var userToDelete = await this.userManager.FindByIdAsync(id);
            var currentUser = await this.userManager.GetUserAsync(this.User);

            if (userToDelete == null || userToDelete == currentUser)
            {
                return this.RedirectToPage("./Index");
            }

            await this.userManager.DeleteAsync(userToDelete);

            return this.RedirectToPage("./Index");
        }
    }
}