namespace ParkingRota.Business.EmailTemplates
{
    using Model;

    public class SingleAllocation : IEmailTemplate
    {
        private readonly Allocation allocation;

        public SingleAllocation(Allocation allocation) => this.allocation = allocation;

        public string To => this.allocation.ApplicationUser.Email;

        public string Subject => $"Space available on {this.allocation.Date.ForDisplayWithDayOfWeek()}";

        public string HtmlBody => $"<p>{this.PlainTextBody}</p>";

        public string PlainTextBody =>
            $"A space has been allocated to you for {this.allocation.Date.ForDisplayWithDayOfWeek()}. " +
            "If you no longer need this space, please remove your request so that it can be given to someone else.";
    }
}