namespace ParkingRota.Pages
{
    using System.Collections.Generic;
    using Calendar;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using NodaTime;

    public class IndexModel : PageModel
    {
        public void OnGet()
        {

        }

        public Calendar Calendar { get; private set; }

        public IDictionary<LocalDate, IReadOnlyList<DisplayRequest>> DisplayRequests { get; private set; }

        public class DisplayRequest
        {
            public DisplayRequest(string fullName, bool isCurrentUser)
            {
                this.FullName = fullName;
                this.IsCurrentUser = isCurrentUser;
            }

            public string FullName { get; }

            public bool IsCurrentUser { get; }
        }
    }
}
