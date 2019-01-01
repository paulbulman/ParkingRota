namespace ParkingRota.UnitTests.Data
{
    using System;
    using System.Linq;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using NodaTime;
    using NodaTime.Testing;
    using NodaTime.Testing.Extensions;
    using ParkingRota.Business.Emails;
    using ParkingRota.Data;
    using Xunit;
    using DataQueueItem = ParkingRota.Data.EmailQueueItem;
    using ModelQueueItem = ParkingRota.Business.Model.EmailQueueItem;

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
                new EmailRepository(context, fakeClock, Mock.Of<IMapper>()).AddToQueue(email);
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

        [Fact]
        public void Test_GetUnsent()
        {
            // Arrange
            var unsentEmail = new DataQueueItem
            {
                To = "a@b.c",
                Subject = "Unsent email subject",
                HtmlBody = "<p>Unsent email body</p>",
                PlainTextBody = "Unsent email body",
                AddedTime = 1.January(2019).At(10, 30, 56).Utc()
            };

            var earlierUnsentEmail = new DataQueueItem
            {
                To = "x@y.z",
                Subject = "Earlier unsent email subject",
                HtmlBody = "<p>Earlier unsent email body</p>",
                PlainTextBody = "Earlier unsent email body",
                AddedTime = 1.January(2019).At(10, 29, 02).Utc()
            };

            var sentEmail = new DataQueueItem
            {
                To = "d@e.f",
                Subject = "Sent email subject",
                HtmlBody = "<p>Sent email body</p>",
                PlainTextBody = "Sent email body",
                AddedTime = 1.January(2019).At(10, 27, 50).Utc(),
                SentTime = 1.January(2019).At(10, 28, 03).Utc()
            };

            using (var context = this.CreateContext())
            {
                context.EmailQueueItems.AddRange(unsentEmail, earlierUnsentEmail, sentEmail);
                context.SaveChanges();
            }

            var mapperConfiguration = new MapperConfiguration(c => { c.CreateMap<DataQueueItem, ModelQueueItem>(); });

            using (var context = this.CreateContext())
            {
                // Act
                var repository = new EmailRepository(
                    context,
                    Mock.Of<IClock>(),
                    new Mapper(mapperConfiguration));

                var result = repository.GetUnsent();

                // Assert
                var unsentEmails = new[] { unsentEmail, earlierUnsentEmail };

                Assert.Equal(unsentEmails.Length, result.Count);

                Assert.True(result.First().AddedTime < result.Last().AddedTime);

                foreach (var expected in unsentEmails)
                {
                    Assert.Single(
                        result,
                        actual =>
                            actual.To == expected.To &&
                            actual.Subject == expected.Subject &&
                            actual.HtmlBody == expected.HtmlBody &&
                            actual.PlainTextBody == expected.PlainTextBody &&
                            actual.AddedTime == expected.AddedTime &&
                            actual.SentTime == null);
                }
            }
        }

        [Fact]
        public void Test_MarkAsSent()
        {
            const string Subject = "Earlier unsent email subject";

            // Arrange
            var unsentEmail = new DataQueueItem
            {
                To = "a@b.c",
                Subject = "Unsent email subject",
                HtmlBody = "<p>Unsent email body</p>",
                PlainTextBody = "Unsent email body",
                AddedTime = 1.January(2019).At(10, 30, 56).Utc()
            };

            var earlierUnsentEmail = new DataQueueItem
            {
                To = "x@y.z",
                Subject = Subject,
                HtmlBody = "<p>Earlier unsent email body</p>",
                PlainTextBody = "Earlier unsent email body",
                AddedTime = 1.January(2019).At(10, 29, 02).Utc()
            };

            var sentEmail = new DataQueueItem
            {
                To = "d@e.f",
                Subject = "Sent email subject",
                HtmlBody = "<p>Sent email body</p>",
                PlainTextBody = "Sent email body",
                AddedTime = 1.January(2019).At(10, 27, 50).Utc(),
                SentTime = 1.January(2019).At(10, 28, 03).Utc()
            };

            using (var context = this.CreateContext())
            {
                context.EmailQueueItems.AddRange(unsentEmail, earlierUnsentEmail, sentEmail);
                context.SaveChanges();
            }

            IMapper mapper = new Mapper(new MapperConfiguration(c =>
            {
                c.CreateMap<DataQueueItem, ModelQueueItem>();
            }));

            var instant = 1.January(2019).At(11, 07, 23).Utc();

            // Act
            using (var context = this.CreateContext())
            {
                var dataUnsentEmail = context.EmailQueueItems.Single(e => e.Subject == Subject);
                var modelUnsentEmail = mapper.Map<ModelQueueItem>(dataUnsentEmail);

                new EmailRepository(context, new FakeClock(instant), Mock.Of<IMapper>()).MarkAsSent(modelUnsentEmail);
            }

            // Assert
            {
                using (var context = this.CreateContext())
                {
                    Assert.Single(context.EmailQueueItems.Where(e => e.DbSentTime == null));

                    var previouslyUnsentEmail = context.EmailQueueItems.Single(e => e.Subject == Subject);

                    Assert.Equal(instant, previouslyUnsentEmail.SentTime);
                }
            }
        }

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(this.contextOptions);

    }
}