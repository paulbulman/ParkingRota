namespace ParkingRota.Areas.Identity.Pages.Account
{
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Business.Model;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;

    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ILogger<LoginModel> logger;

        public LoginModel(
            IHttpContextAccessor httpContextAccessor,
            SignInManager<ApplicationUser> signInManager, 
            ILogger<LoginModel> logger)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.signInManager = signInManager;
            this.logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(this.ErrorMessage))
            {
                this.ModelState.AddModelError(string.Empty, this.ErrorMessage);
            }

            this.ReturnUrl = returnUrl ?? this.Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? this.Url.Content("~/");

            if (this.ModelState.IsValid)
            {
                var originatingIpAddress = this.httpContextAccessor.GetOriginatingIpAddress();

                var result = await this.signInManager.PasswordSignInAsync(
                    this.Input.Email, this.Input.Password, this.Input.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    this.logger.LogInformation(
                        $"User {this.Input.Email} logged in from IP address {originatingIpAddress}.");

                    return this.LocalRedirect(returnUrl);
                }

                if (result.RequiresTwoFactor)
                {
                    this.logger.LogInformation(
                        $"User {this.Input.Email} logged in from IP address {originatingIpAddress}.");

                    return this.RedirectToPage(
                        "./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = this.Input.RememberMe });
                }

                this.logger.LogInformation(
                    $"Failed login attempt for user {this.Input.Email} from IP address {originatingIpAddress}.");

                this.ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            // If we got this far, something failed, redisplay form
            return this.Page();
        }
    }
}
