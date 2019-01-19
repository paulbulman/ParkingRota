namespace ParkingRota.Areas.Identity.Pages.Account
{
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Business;
    using Business.Emails;
    using Business.Model;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;

    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IRegistrationTokenValidator registrationTokenValidator;
        private readonly IPasswordBreachChecker passwordBreachChecker;
        private readonly ILogger<RegisterModel> logger;
        private readonly IEmailRepository emailRepository;

        public RegisterModel(
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            IRegistrationTokenValidator registrationTokenValidator,
            IPasswordBreachChecker passwordBreachChecker,
            ILogger<RegisterModel> logger,
            IEmailRepository emailRepository)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
            this.registrationTokenValidator = registrationTokenValidator;
            this.passwordBreachChecker = passwordBreachChecker;
            this.logger = logger;
            this.emailRepository = emailRepository;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(50)]
            [Display(Name = "First name")]
            public string FirstName { get; set; }

            [Required]
            [StringLength(50)]
            [Display(Name = "Last name")]
            public string LastName { get; set; }

            [Required]
            [StringLength(10)]
            [Display(Name = "Car registration number")]
            public string CarRegistrationNumber { get; set; }

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

        public async Task<IActionResult> OnPostAsync()
        {
            if (this.ModelState.IsValid)
            {
                var registrationTokenIsValid =
                    this.registrationTokenValidator.TokenIsValid(this.Input.RegistrationToken);

                if (!registrationTokenIsValid)
                {
                    this.ModelState.AddModelError(
                        $"{nameof(this.Input)}.{nameof(this.Input.RegistrationToken)}",
                        "Registration token not valid.");
                }

                var passwordIsBreached = await this.passwordBreachChecker.PasswordIsBreached(this.Input.Password);

                if (passwordIsBreached)
                {
                    this.ModelState.AddModelError(
                        $"{nameof(this.Input)}.{nameof(this.Input.Password)}",
                        "Password is known to have been compromised in a data breach.");
                }

                if (registrationTokenIsValid && !passwordIsBreached)
                {
                    var user = CreateApplicationUser(this.Input);

                    var result = await this.userManager.CreateAsync(user, this.Input.Password);

                    if (result.Succeeded)
                    {
                        this.logger.LogInformation($"Created user with Id {user.Id}.");

                        var emailConfirmationToken = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = this.Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { userId = user.Id, code = emailConfirmationToken },
                            protocol: "https");

                        var ipAddress = this.httpContextAccessor.GetOriginatingIpAddress();

                        var confirmationEmail = new ConfirmEmailAddress(this.Input.Email, callbackUrl, ipAddress);

                        this.emailRepository.AddToQueue(confirmationEmail);

                        return this.RedirectToPage("/RegisterSuccess");
                    }

                    foreach (var error in result.Errors)
                    {
                        this.ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return this.Page();
        }

        private static ApplicationUser CreateApplicationUser(InputModel input)
        {
            const decimal DefaultCommuteDistance = 9.99m;

            return new ApplicationUser
            {
                FirstName = input.FirstName,
                LastName = input.LastName,
                CarRegistrationNumber = input.CarRegistrationNumber,
                CommuteDistance = DefaultCommuteDistance,
                UserName = input.Email,
                Email = input.Email
            };
        }
    }
}
