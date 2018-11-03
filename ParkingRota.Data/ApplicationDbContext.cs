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

        public DbSet<BankHoliday> BankHolidays { get; set; }

        public DbSet<RegistrationToken> RegistrationTokens { get; set; }

        public DbSet<Request> Requests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>().Property(a => a.CommuteDistance).HasColumnType("decimal(18,2)");

            builder.Entity<BankHoliday>().HasIndex(b => b.DbDate).IsUnique();
            builder.Entity<BankHoliday>().Property(b => b.DbDate).HasColumnName("Date");
            builder.Entity<BankHoliday>().Ignore(b => b.Date);

            builder.Entity<RegistrationToken>().Property(t => t.DbExpiryTime).HasColumnName("ExpiryTime");
            builder.Entity<RegistrationToken>().Ignore(t => t.ExpiryTime);

            builder.Entity<Request>().HasIndex(r => new { r.ApplicationUserId, r.DbDate }).IsUnique();
            builder.Entity<Request>().Property(r => r.DbDate).HasColumnName("Date");
            builder.Entity<Request>().Ignore(r => r.Date);
        }
    }
}
