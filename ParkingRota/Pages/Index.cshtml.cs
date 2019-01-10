namespace ParkingRota.Pages
{
    using Business.Model;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    public class IndexModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> signInManager;

        public IndexModel(SignInManager<ApplicationUser> signInManager) => this.signInManager = signInManager;

        public IActionResult OnGet() => this.signInManager.IsSignedIn(this.User) ?
            this.RedirectToPage("/Summary") :
            (IActionResult)this.Page();
    }
}