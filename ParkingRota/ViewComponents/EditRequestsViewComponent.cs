namespace ParkingRota.ViewComponents
{
    using System.Linq;
    using Business;
    using Business.Model;
    using Calendar;
    using Microsoft.AspNetCore.Mvc;

    public class EditRequestsViewComponent : ViewComponent
    {
        private readonly IDateCalculator dateCalculator;
        private readonly IRequestRepository requestRepository;

        public EditRequestsViewComponent(IDateCalculator dateCalculator, IRequestRepository requestRepository)
        {
            this.dateCalculator = dateCalculator;
            this.requestRepository = requestRepository;
        }

        public IViewComponentResult Invoke(string selectedUserId)
        {
            var activeDates = this.dateCalculator.GetActiveDates();

            var requests = this.requestRepository.GetRequests(activeDates.First(), activeDates.Last());

            var calendarData = activeDates.ToDictionary(
                d => d,
                d => requests.Any(r => r.Date == d && r.ApplicationUser.Id == selectedUserId));

            var calendar = Calendar<bool>.Create(calendarData);

            return this.View(new EditRequestsViewModel(selectedUserId, calendar));
        }

        public class EditRequestsViewModel
        {
            public EditRequestsViewModel(string selectedUserId, Calendar<bool> calendar)
            {
                this.SelectedUserId = selectedUserId;
                this.Calendar = calendar;
            }

            public string SelectedUserId { get; }

            public Calendar<bool> Calendar { get; }
        }
    }
}