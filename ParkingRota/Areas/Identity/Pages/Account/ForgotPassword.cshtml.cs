namespace ParkingRota.Areas.Identity.Pages.Account
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;
    using Business.Emails;
    using Business.Model;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailRepository emailRepository;

        public ForgotPasswordModel(
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            IEmailRepository emailRepository)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
            this.emailRepository = emailRepository;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (this.ModelState.IsValid)
            {
                // Deliberately slow this method down slightly to avoid leaking information via time taken
                await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(200)));

                var user = await this.userManager.FindByEmailAsync(this.Input.Email);
                if (user == null || !(await this.userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return this.RedirectToPage("./ForgotPasswordConfirmation");
                }

                var code = await this.userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = this.Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { code },
                    protocol: this.Request.Scheme);

                var ipAddress = this.httpContextAccessor.GetOriginatingIpAddress();

                var resetPasswordEmail = new ResetPassword(this.Input.Email, callbackUrl, ipAddress);

                var recentlySent = this.emailRepository
                    .GetRecent()
                    .Any(e => e.To == resetPasswordEmail.To && e.Subject == resetPasswordEmail.Subject);

                if (!recentlySent)
                {
                    this.emailRepository.AddToQueue(resetPasswordEmail);
                }

                return this.RedirectToPage("./ForgotPasswordConfirmation");
            }

            return this.Page();
        }
    }
}
