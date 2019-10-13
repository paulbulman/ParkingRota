namespace ParkingRota.UnitTests.Data
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime;
    using ParkingRota.Business;
    using ParkingRota.Data;
    using ApplicationUser = ParkingRota.Business.Model.ApplicationUser;
    using ScheduledTaskType = ParkingRota.Business.Model.ScheduledTaskType;

    public class DatabaseSeeder
    {
        private readonly ServiceProvider serviceProvider;

        public DatabaseSeeder(ServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;

        public Allocation Allocation(ApplicationUser applicationUser, LocalDate date)
        {
            var allocation = new Allocation
            {
                ApplicationUserId = applicationUser.Id,
                Date = date
            };

            using (var scope = this.serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                context.Allocations.Add(allocation);
                context.SaveChanges();
            }

            allocation.ApplicationUser = applicationUser;

            return allocation;
        }

        public async Task<ApplicationUser> ApplicationUser(string email, bool isTeamLeader = false, bool isVisitor = false)
        {
            var userName = email.Split("@").First();

            var firstName = userName.Split(".").First();
            var lastName = userName.Split(".").Last();

            var applicationUser = new ApplicationUser
            {
                UserName = userName,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                IsVisitor = isVisitor
            };

            using (var scope = this.serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                await userManager.CreateAsync(applicationUser);

                if (isTeamLeader)
                {
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                    if (!await roleManager.RoleExistsAsync(UserRole.TeamLeader))
                    {
                        await roleManager.CreateAsync(new IdentityRole(UserRole.TeamLeader));
                    }

                    await userManager.AddToRoleAsync(applicationUser, UserRole.TeamLeader);
                }
            }

            return applicationUser;
        }

        public BankHoliday BankHoliday(LocalDate date)
        {
            var bankHoliday = new BankHoliday { Date = date };

            using (var scope = this.serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                context.BankHolidays.Add(bankHoliday);
                context.SaveChanges();
            }

            return bankHoliday;
        }

        public RegistrationToken RegistrationToken(string token, Instant expiryTime)
        {
            var registrationToken = new RegistrationToken { Token = token, ExpiryTime = expiryTime };

            using (var scope = this.serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                context.RegistrationTokens.Add(registrationToken);
                context.SaveChanges();
            }

            return registrationToken;
        }

        public Request Request(ApplicationUser applicationUser, LocalDate date)
        {
            var request = new Request
            {
                ApplicationUserId = applicationUser.Id,
                Date = date
            };

            using (var scope = this.serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                context.Requests.Add(request);
                context.SaveChanges();
            }

            request.ApplicationUser = applicationUser;

            return request;
        }

        public Reservation Reservation(ApplicationUser applicationUser, LocalDate date, int order)
        {
            var reservation = new Reservation
            {
                ApplicationUserId = applicationUser.Id,
                Date = date,
                Order = order
            };

            using (var scope = this.serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                context.Reservations.Add(reservation);
                context.SaveChanges();
            }

            reservation.ApplicationUser = applicationUser;

            return reservation;
        }

        public ScheduledTask ScheduledTask(Instant nextRunTime, ScheduledTaskType scheduledTaskType)
        {
            var scheduledTask = new ScheduledTask { NextRunTime = nextRunTime, ScheduledTaskType = scheduledTaskType };

            using (var scope = this.serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                context.ScheduledTasks.Add(scheduledTask);
                context.SaveChanges();
            }

            return scheduledTask;
        }
    }
}