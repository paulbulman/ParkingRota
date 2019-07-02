namespace ParkingRota.ViewComponents
{
    using System.Collections.Generic;
    using System.Linq;
    using Business;
    using Business.Model;
    using Calendar;
    using Microsoft.AspNetCore.Mvc;
    using NodaTime;

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
                d => CreateDayRequest(requests, d, selectedUserId, activeDates));

            var calendar = Calendar<DayRequest>.Create(calendarData);

            return this.View(new EditRequestsViewModel(selectedUserId, calendar));
        }

        private static DayRequest CreateDayRequest(
            IReadOnlyList<Request> requests,
            LocalDate date,
            string selectedUserId,
            IReadOnlyList<LocalDate> activeDates)
        {
            var isSelected = requests.Any(r => r.Date == date && r.ApplicationUser.Id == selectedUserId);
            var isNextMonth = date.Month == activeDates.Last().Month;

            return new DayRequest(isSelected, isNextMonth);
        }

        public class EditRequestsViewModel
        {
            public EditRequestsViewModel(string selectedUserId, Calendar<DayRequest> calendar)
            {
                this.SelectedUserId = selectedUserId;
                this.Calendar = calendar;
            }

            public string SelectedUserId { get; }

            public Calendar<DayRequest> Calendar { get; }
        }

        public class DayRequest
        {
            public DayRequest(bool isSelected, bool isNextMonth)
            {
                this.IsSelected = isSelected;
                this.IsNextMonth = isNextMonth;
            }

            public bool IsSelected { get; }
            
            public bool IsNextMonth { get; }
        }
    }
}