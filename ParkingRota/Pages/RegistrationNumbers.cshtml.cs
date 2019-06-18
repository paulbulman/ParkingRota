namespace ParkingRota.Pages
{
    using System.Collections.Generic;
    using System.Linq;
    using Business.Model;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    public class RegistrationNumbersModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;

        public RegistrationNumbersModel(UserManager<ApplicationUser> userManager) => this.userManager = userManager;

        public IReadOnlyList<RegistrationNumberRecord> RegistrationNumberRecords { get; private set; }

        public void OnGet() =>
            this.RegistrationNumberRecords = this.userManager.Users
                .Where(u => !u.IsVisitor)
                .ToArray()
                .SelectMany(CreateRegistrationNumberRecords)
                .OrderBy(r => r.CarRegistrationNumber)
                .ToArray();

        private static IEnumerable<RegistrationNumberRecord> CreateRegistrationNumberRecords(ApplicationUser user)
        {
            var registrationNumberRecords = new List<RegistrationNumberRecord>
            {
                new RegistrationNumberRecord(user.FullName, user.CarRegistrationNumber)
            };

            if (!string.IsNullOrEmpty(user.AlternativeCarRegistrationNumber))
            {
                registrationNumberRecords.Add(
                    new RegistrationNumberRecord(user.FullName, user.AlternativeCarRegistrationNumber));
            }

            return registrationNumberRecords;
        }

        public class RegistrationNumberRecord
        {
            public RegistrationNumberRecord(string fullName, string carRegistrationNumber)
            {
                this.FullName = fullName;
                this.CarRegistrationNumber = carRegistrationNumber;
            }

            public string FullName { get; }

            public string CarRegistrationNumber { get; }
        }
    }
}