﻿namespace ParkingRota.UnitTests.Business
{
    using System.Threading.Tasks;
    using Moq;
    using ParkingRota.Business;
    using ParkingRota.Business.Emails;
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

            var mockEmailSender = new Mock<IEmailSender>(MockBehavior.Strict);
            mockEmailSender
                .Setup(s => s.Send(It.IsAny<IEmail>()))
                .Returns(Task.CompletedTask);

            // Act
            var emailProcessor = new EmailProcessor(mockEmailRepository.Object, mockEmailSender.Object);

            await emailProcessor.SendPending();

            // Assert
            foreach (var emailQueueItem in unsentEmails)
            {
                mockEmailSender.Verify(
                    s => s.Send(It.Is<IEmail>(e =>
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