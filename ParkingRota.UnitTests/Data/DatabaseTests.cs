namespace ParkingRota.UnitTests.Data
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using ParkingRota.Data;

    public class DatabaseTests
    {
        private readonly DbContextOptions<ApplicationDbContext> contextOptions;

        protected DatabaseTests() =>
            this.contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        protected ApplicationDbContext CreateContext() => new ApplicationDbContext(this.contextOptions);
    }
}