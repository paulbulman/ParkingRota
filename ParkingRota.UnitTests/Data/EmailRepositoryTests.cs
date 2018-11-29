namespace ParkingRota.UnitTests.Data
{
    using System;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using NodaTime.Testing;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.Emails;
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

            var email = Mock.Of<IEmail>(e =>
                e.To == EmailAddress &&
                e.Subject == Subject &&
                e.HtmlBody == HtmlBody &&
                e.PlainTextBody == PlainTextBody);

            var instant = 25.November(2018).At(12, 37, 12).Utc();

            var fakeClock = new FakeClock(instant);

            // Act
            using (var context = this.CreateContext())
            {
                new EmailRepository(context, fakeClock).AddToQueue(email);
            }

            using (var context = this.CreateContext())
            {
                var emails = context.EmailQueueItems.ToArray();

                Assert.Single(emails);

                var actual = emails[0];

                Assert.Equal(EmailAddress, actual.To);
                Assert.Equal(Subject, actual.Subject);
                Assert.Equal(PlainTextBody, actual.PlainTextBody);
                Assert.Equal(HtmlBody, actual.HtmlBody);
                Assert.Equal(instant, actual.AddedTime);
            }
        }

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(this.contextOptions);

    }
}