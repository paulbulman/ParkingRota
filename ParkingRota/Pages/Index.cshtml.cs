namespace ParkingRota.Pages
{
    using System.Threading.Tasks;
    using Areas.Identity.Pages.Account;
    using Business.Model;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;

    public class IndexModel : PageModel
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<LoginModel> logger;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;

        public IndexModel(
            IHttpContextAccessor httpContextAccessor,
            ILogger<LoginModel> logger,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (this.signInManager.IsSignedIn(this.User))
            {
                var user = await this.userManager.GetUserAsync(this.User);

                var originatingIpAddress = this.httpContextAccessor.GetOriginatingIpAddress();

                this.logger.LogInformation(
                    $"User {user.Email} logged in automatically from IP address {originatingIpAddress}.");

                return this.RedirectToPage("/Summary");
            }

            return this.Page();
        }
    }
}