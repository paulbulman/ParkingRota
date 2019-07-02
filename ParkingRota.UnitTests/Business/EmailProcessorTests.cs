namespace ParkingRota.UnitTests.Business
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Moq;
    using ParkingRota.Business;
    using ParkingRota.Business.EmailSenders;
    using ParkingRota.Business.EmailTemplates;
    using ParkingRota.Business.Model;
    using Xunit;

    public static class EmailProcessorTests
    {
        [Fact]
        public static async Task Test_SendPending()
        {
            // Arrange
            var unsentEmail = new EmailQueueItem
            {
                To = "a@b.c",
                Subject = "Unsent email subject",
                HtmlBody = "<p>Unsent email body</p>",
                PlainTextBody = "Unsent email body"
            };

            var otherUnsentEmail = new EmailQueueItem
            {
                To = "x@y.z",
                Subject = "Other unsent email subject",
                HtmlBody = "<p>Other unsent email body</p>",
                PlainTextBody = "Other unsent email body"
            };

            var unsentEmails = new[] { unsentEmail, otherUnsentEmail };

            var mockEmailRepository = new Mock<IEmailRepository>(MockBehavior.Strict);
            mockEmailRepository
                .Setup(r => r.GetUnsent())
                .Returns(unsentEmails);
            mockEmailRepository
                .Setup(r => r.MarkAsSent(It.IsAny<EmailQueueItem>()));

            var disabledMockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            disabledMockEmailSender
                .SetupGet(s => s.CanSend)
                .Returns(false);

            var enabledMockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            enabledMockEmailSender
                .SetupGet(s => s.CanSend)
                .Returns(true);
            enabledMockEmailSender
                .Setup(s => s.Send(It.IsAny<IEmailTemplate>()))
                .Returns(Task.CompletedTask);

            // Act
            var emailProcessor = new EmailProcessor(
                mockEmailRepository.Object,
                new[] { disabledMockEmailSender.Object, enabledMockEmailSender.Object },
                Mock.Of<ILogger<EmailProcessor>>());

            await emailProcessor.SendPending();

            // Assert
            foreach (var emailQueueItem in unsentEmails)
            {
                enabledMockEmailSender.Verify(
                    s => s.Send(It.Is<IEmailTemplate>(e =>
                        e.To == emailQueueItem.To &&
                        e.Subject == emailQueueItem.Subject &&
                        e.HtmlBody == emailQueueItem.HtmlBody &&
                        e.PlainTextBody == emailQueueItem.PlainTextBody)),
                    Times.Once);

                mockEmailRepository.Verify(
                    r => r.MarkAsSent(emailQueueItem),
                    Times.Once);
            }
        }
    }
}