namespace ParkingRota.UnitTests
{
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using ParkingRota.Business.Model;

    public static class Create
    {
        public static IReadOnlyList<Allocation> Allocations(IEnumerable<ApplicationUser> users, LocalDate date) =>
            users.Select(u => new Allocation { ApplicationUser = u, Date = date }).ToArray();

        public static IReadOnlyList<Request> Requests(IEnumerable<ApplicationUser> users, LocalDate date) =>
            users.Select(u => new Request { ApplicationUser = u, Date = date }).ToArray();

        public static IReadOnlyList<ApplicationUser> Users(params string[] fullNames) =>
            fullNames.Select(User).ToArray();

        private static ApplicationUser User(string fullName)
        {
            var names = fullName.Split(" ");

            return new ApplicationUser { FirstName = names.First(), LastName = names.Last() };
        }
    }
}