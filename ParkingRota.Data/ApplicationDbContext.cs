namespace ParkingRota.Data
{
    using Business.Model;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<RegistrationToken> RegistrationTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>().Property(a => a.CommuteDistance).HasColumnType("decimal(18,2)");

            builder.Entity<RegistrationToken>().Property(t => t.DbExpiryTime).HasColumnName("ExpiryTime");
            builder.Entity<RegistrationToken>().Ignore(t => t.ExpiryTime);
        }
    }
}
