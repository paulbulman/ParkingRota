namespace ParkingRota.IntegrationTests
{
    using Data;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using ParkingRota.Business.Model;

    public class DatabaseWebApplicationFactory<TProgram> : WebApplicationFactory<Program>
    {
        public const string DefaultUserId = "b35d8fae-6e76-486d-9255-4ea5b68527b1";
        public const string DefaultEmailAddress = "anneother@gmail.com";
        public const string DefaultPassword = "9Ft6M%";

        private const string DefaultPasswordHash =
            "AQAAAAEAACcQAAAAEGe/qgvKfGP5QOeQnC2YF5Fzphi2AvOD71xUXnzfW4yQfuuEGJ4qrdzt9bwESjN4Mw==";

        protected override void ConfigureWebHost(IWebHostBuilder builder) =>
            builder.ConfigureServices(services =>
            {
                var contextServiceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                    options.UseInternalServiceProvider(contextServiceProvider);
                });

                using (var serviceScope = services.BuildServiceProvider().CreateScope())
                {
                    var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    context.Database.EnsureCreated();

                    var applicationUser = new ApplicationUser
                    {
                        Id = DefaultUserId,
                        UserName = DefaultEmailAddress,
                        NormalizedUserName = DefaultEmailAddress.ToUpper(),
                        Email = DefaultEmailAddress,
                        NormalizedEmail = DefaultEmailAddress.ToUpper(),
                        PasswordHash = DefaultPasswordHash,
                        SecurityStamp = "DI5SLUUOBZMZJ3ROV6CKOO673JJFF72E",
                        ConcurrencyStamp = "1837d1c1-393b-46ba-9397-578fca593f9d",
                        CarRegistrationNumber = "W 789 XYZ",
                        CommuteDistance = 9.99m,
                        FirstName = "Anne",
                        LastName = "Other",
                        EmailConfirmed = true
                    };

                    context.Users.Add(applicationUser);
                    context.SaveChanges();
                }
            });
    }
}