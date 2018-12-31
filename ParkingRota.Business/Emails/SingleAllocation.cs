namespace ParkingRota.Business.Emails
{
    using Model;

    public class SingleAllocation : IEmail
    {
        private readonly Allocation allocation;

        public SingleAllocation(Allocation allocation) => this.allocation = allocation;

        public string To => this.allocation.ApplicationUser.Email;

        public string Subject => $"Space available on {this.allocation.Date.ForDisplay()}";

        public string HtmlBody => $"<p>{this.PlainTextBody}</p>";

        public string PlainTextBody =>
            $"A space has been allocated to you for {this.allocation.Date.ForDisplay()}. " +
            "If you no longer need this space, please remove your request so that it can be given to someone else.";
    }
}