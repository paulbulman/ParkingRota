namespace ParkingRota.Data
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<RegistrationToken> RegistrationTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RegistrationToken>().Property(t => t.DbExpiryTime).HasColumnName("ExpiryTime");
            builder.Entity<RegistrationToken>().Ignore(t => t.ExpiryTime);
        }
    }
}
