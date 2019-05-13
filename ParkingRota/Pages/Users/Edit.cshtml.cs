namespace ParkingRota.Pages.Users
{
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;
    using Business.Model;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    public class EditModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;

        public EditModel(UserManager<ApplicationUser> userManager) => this.userManager = userManager;

        [BindProperty]
        public InputModel Input { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var userToEdit = await this.userManager.FindByIdAsync(id);
            var currentUser = await this.userManager.GetUserAsync(this.User);

            if (userToEdit == null || userToEdit == currentUser)
            {
                return this.NotFound();
            }

            this.Input = new InputModel
            {
                Id = userToEdit.Id,
                FirstName = userToEdit.FirstName,
                LastName = userToEdit.LastName,
                CarRegistrationNumber = userToEdit.CarRegistrationNumber,
                AlternativeCarRegistrationNumber = userToEdit.AlternativeCarRegistrationNumber,
                CommuteDistance = userToEdit.CommuteDistance
            };

            return this.Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!this.ModelState.IsValid)
            {
                return this.Page();
            }

            var userToEdit = await this.userManager.FindByIdAsync(this.Input.Id);
            var currentUser = await this.userManager.GetUserAsync(this.User);

            if (userToEdit == null || userToEdit == currentUser)
            {
                return this.RedirectToPage("./Index");
            }

            userToEdit.FirstName = this.Input.FirstName;
            userToEdit.LastName = this.Input.LastName;
            userToEdit.CarRegistrationNumber = this.Input.CarRegistrationNumber;
            userToEdit.AlternativeCarRegistrationNumber = this.Input.AlternativeCarRegistrationNumber;
            userToEdit.CommuteDistance = this.Input.CommuteDistance;

            await this.userManager.UpdateAsync(userToEdit);

            return this.RedirectToPage("./Index");
        }

        public class InputModel
        {
            [HiddenInput]
            public string Id { get; set; }

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

            [Required]
            [Display(Name = "Commute distance (miles)")]
            public decimal? CommuteDistance { get; set; }
        }
    }
}