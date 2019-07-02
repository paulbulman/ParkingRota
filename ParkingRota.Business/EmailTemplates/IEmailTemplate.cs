namespace ParkingRota.Business.EmailTemplates
{
    public interface IEmailTemplate
    {
        string To { get; }

        string Subject { get; }

        string HtmlBody { get; }

        string PlainTextBody { get; }
    }
}