namespace ParkingRota.UnitTests.Data
{
    using System;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using NodaTime.Testing;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Data;
    using Xunit;

    public class EmailRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> contextOptions;

        public EmailRepositoryTests() =>
            this.contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        [Fact]
        public void Test_AddToQueue()
        {
            const string EmailAddress = "a@b.com";
            const string Subject = "Test subject";
            const string HtmlBody = "<p>Test body</p>";
            const string PlainTextBody = "Test body";

            var instant = 25.November(2018).At(12, 37, 12).Utc();

            var fakeClock = new FakeClock(instant);

            // Act
            using (var context = this.CreateContext())
            {
                new EmailRepository(context, fakeClock).AddToQueue(EmailAddress, Subject, HtmlBody, PlainTextBody);
            }

            using (var context = this.CreateContext())
            {
                var emails = context.EmailQueueItems.ToArray();

                Assert.Single(emails);

                var email = emails[0];

                Assert.Equal(EmailAddress, email.To);
                Assert.Equal(Subject, email.Subject);
                Assert.Equal(PlainTextBody, email.PlainTextBody);
                Assert.Equal(HtmlBody, email.HtmlBody);
                Assert.Equal(instant, email.AddedTime);
            }
        }

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(this.contextOptions);

    }
}