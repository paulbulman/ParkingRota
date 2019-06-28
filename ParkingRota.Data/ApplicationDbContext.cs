namespace ParkingRota.Data
{
    using Business.Model;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    public interface IApplicationDbContext
    {
        DbSet<Allocation> Allocations { get; set; }

        DbSet<BankHoliday> BankHolidays { get; set; }

        DbSet<EmailQueueItem> EmailQueueItems { get; set; }

        DbSet<RegistrationToken> RegistrationTokens { get; set; }

        DbSet<Request> Requests { get; set; }

        DbSet<Reservation> Reservations { get; set; }

        DbSet<ScheduledTask> ScheduledTasks { get; set; }

        DbSet<SystemParameterList> SystemParameterLists { get; set; }

        int SaveChanges();
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Allocation> Allocations { get; set; }

        public DbSet<BankHoliday> BankHolidays { get; set; }

        public DbSet<EmailQueueItem> EmailQueueItems { get; set; }

        public DbSet<RegistrationToken> RegistrationTokens { get; set; }

        public DbSet<Request> Requests { get; set; }

        public DbSet<Reservation> Reservations { get; set; }

        public DbSet<ScheduledTask> ScheduledTasks { get; set; }

        public DbSet<SystemParameterList> SystemParameterLists { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Allocation>().HasIndex(a => new { a.ApplicationUserId, a.DbDate }).IsUnique();
            builder.Entity<Allocation>().Property(a => a.DbDate).HasColumnName("Date");
            builder.Entity<Allocation>().Ignore(a => a.Date);

            builder.Entity<ApplicationUser>().Property(a => a.CommuteDistance).HasColumnType("decimal(18,2)");

            builder.Entity<BankHoliday>().HasIndex(b => b.DbDate).IsUnique();
            builder.Entity<BankHoliday>().Property(b => b.DbDate).HasColumnName("Date");
            builder.Entity<BankHoliday>().Ignore(b => b.Date);

            builder.Entity<EmailQueueItem>().Property(e => e.DbAddedTime).HasColumnName("AddedTime");
            builder.Entity<EmailQueueItem>().Ignore(e => e.AddedTime);
            builder.Entity<EmailQueueItem>().Property(e => e.DbSentTime).HasColumnName("SentTime");
            builder.Entity<EmailQueueItem>().Ignore(e => e.SentTime);

            builder.Entity<RegistrationToken>().Property(t => t.DbExpiryTime).HasColumnName("ExpiryTime");
            builder.Entity<RegistrationToken>().Ignore(t => t.ExpiryTime);

            builder.Entity<Request>().HasIndex(r => new { r.ApplicationUserId, r.DbDate }).IsUnique();
            builder.Entity<Request>().Property(r => r.DbDate).HasColumnName("Date");
            builder.Entity<Request>().Ignore(r => r.Date);

            builder.Entity<Reservation>().HasIndex(r => new { r.ApplicationUserId, r.DbDate, r.Order }).IsUnique();
            builder.Entity<Reservation>().Property(r => r.DbDate).HasColumnName("Date");
            builder.Entity<Reservation>().Ignore(r => r.Date);

            builder.Entity<ScheduledTask>().HasIndex(t => t.ScheduledTaskType).IsUnique();
            builder.Entity<ScheduledTask>().Property(t => t.DbNextRunTime).HasColumnName("NextRunTime");
            builder.Entity<ScheduledTask>().Ignore(t => t.NextRunTime);

            builder.Entity<SystemParameterList>().Property(t => t.DbLastServiceRunTime).HasColumnName("LastServiceRunTime");
            builder.Entity<SystemParameterList>().Ignore(t => t.LastServiceRunTime);

            builder.Entity<SystemParameterList>().Property(p => p.NearbyDistance).HasColumnType("decimal(18,2)");
        }
    }
}
