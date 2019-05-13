namespace ParkingRota.Pages.Users
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Business.Emails;
    using Business.Model;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using NodaTime;

    public class CreateModel : PageModel
    {
        private readonly IClock clock;
        private readonly IEmailRepository emailRepository;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IRegistrationTokenRepository registrationTokenRepository;

        public CreateModel(
            IClock clock,
            IEmailRepository emailRepository,
            IHttpContextAccessor httpContextAccessor,
            IRegistrationTokenRepository registrationTokenRepository)
        {
            this.clock = clock;
            this.emailRepository = emailRepository;
            this.httpContextAccessor = httpContextAccessor;
            this.registrationTokenRepository = registrationTokenRepository;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public IActionResult OnPost()
        {
            if (!this.ModelState.IsValid)
            {
                return this.Page();
            }

            var token = Guid.NewGuid().ToString();

            this.registrationTokenRepository.AddRegistrationToken(
                new RegistrationToken
                {
                    Token = token,
                    ExpiryTime = this.clock.GetCurrentInstant().Plus(Duration.FromHours(24))
                });

            var callbackUrl = this.Url.Page(
                "/Account/Register",
                pageHandler: null,
                values: new { area = "Identity", registrationToken = token },
                protocol: "https");

            var ipAddress = this.httpContextAccessor.GetOriginatingIpAddress();

            this.emailRepository.AddToQueue(new Signup(this.Input.Email, callbackUrl, ipAddress));

            this.ModelState.Clear();

            this.StatusMessage = "Email will be sent.";

            return this.RedirectToPage();
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [EmailAddress]
            [Display(Name = "Confirm email")]
            [Compare("Email", ErrorMessage = "The email and confirmation email do not match.")]
            public string ConfirmEmail { get; set; }
        }
    }
}