namespace ParkingRota.Business.Emails
{
    public interface IEmail
    {
        string To { get; }

        string Subject { get; }

        string HtmlBody { get; }

        string PlainTextBody { get; }
    }
}