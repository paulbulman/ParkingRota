namespace ParkingRota.Areas.Identity.Pages.Account.Manage
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Business.Model;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public IndexModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

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

            [StringLength(10)]
            [Display(Name = "Alternative car registration number")]
            public string AlternativeCarRegistrationNumber { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await this.userManager.GetUserAsync(this.User);
            if (user == null)
            {
                return this.NotFound($"Unable to load user with ID '{this.userManager.GetUserId(this.User)}'.");
            }

            var userName = await this.userManager.GetUserNameAsync(user);

            this.Username = userName;

            this.Input = new InputModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                CarRegistrationNumber = user.CarRegistrationNumber,
                AlternativeCarRegistrationNumber = user.AlternativeCarRegistrationNumber
            };

            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!this.ModelState.IsValid)
            {
                return this.Page();
            }

            var user = await this.userManager.GetUserAsync(this.User);
            if (user == null)
            {
                return this.NotFound($"Unable to load user with ID '{this.userManager.GetUserId(this.User)}'.");
            }

            if (this.Input.FirstName != user.FirstName ||
                this.Input.LastName != user.LastName ||
                this.Input.CarRegistrationNumber != user.CarRegistrationNumber ||
                this.Input.AlternativeCarRegistrationNumber != user.AlternativeCarRegistrationNumber)
            {
                user.FirstName = this.Input.FirstName;
                user.LastName = this.Input.LastName;
                user.CarRegistrationNumber = this.Input.CarRegistrationNumber;
                user.AlternativeCarRegistrationNumber = this.Input.AlternativeCarRegistrationNumber;

                var updateUserResult = await this.userManager.UpdateAsync(user);
                if (!updateUserResult.Succeeded)
                {
                    var userId = await this.userManager.GetUserIdAsync(user);
                    throw new InvalidOperationException($"Unexpected error occurred updating user with ID '{userId}'.");
                }
            }

            await this.signInManager.RefreshSignInAsync(user);
            this.StatusMessage = "Profile updated.";
            return this.RedirectToPage();
        }
    }
}
