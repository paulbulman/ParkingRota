namespace ParkingRota.Areas.Identity.Pages.Account
{
    using System.ComponentModel.DataAnnotations;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Business;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;

    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IRegistrationTokenValidator registrationTokenValidator;
        private readonly ILogger<RegisterModel> logger;
        private readonly IEmailSender emailSender;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IRegistrationTokenValidator registrationTokenValidator,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            this.userManager = userManager;
            this.registrationTokenValidator = registrationTokenValidator;
            this.signInManager = signInManager;
            this.logger = logger;
            this.emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [Display(Name = "Registration token")]
            public string RegistrationToken { get; set; }
        }

        public void OnGet(string returnUrl = null) => this.ReturnUrl = returnUrl;

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? this.Url.Content("~/");
            if (this.ModelState.IsValid)
            {
                if (this.registrationTokenValidator.TokenIsValid(this.Input.RegistrationToken))
                {
                    var user = new IdentityUser { UserName = this.Input.Email, Email = this.Input.Email };

                    var result = await this.userManager.CreateAsync(user, this.Input.Password);

                    if (result.Succeeded)
                    {
                        this.logger.LogInformation($"Created user with Id {user.Id}.");

                        var emailConfirmationToken = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = this.Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { userId = user.Id, code = emailConfirmationToken },
                            protocol: this.Request.Scheme);

                        var encodedCallbackUrl = HtmlEncoder.Default.Encode(callbackUrl);
                        var emailBody =
                            $"Please confirm your account by <a href='{encodedCallbackUrl}'>clicking here</a>.";

                        await this.emailSender.SendEmailAsync(this.Input.Email, "Confirm your email", emailBody);

                        await this.signInManager.SignInAsync(user, isPersistent: false);

                        return this.LocalRedirect(returnUrl);
                    }

                    foreach (var error in result.Errors)
                    {
                        this.ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                else
                {
                    this.ModelState.AddModelError(
                        $"{nameof(this.Input)}.{nameof(this.Input.RegistrationToken)}",
                        "Registration token not valid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return this.Page();
        }
    }
}
