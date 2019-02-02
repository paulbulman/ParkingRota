namespace ParkingRota.Business
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using NodaTime;
    using NodaTime.Text;

    public interface IBankHolidayFetcher
    {
        Task<IReadOnlyList<LocalDate>> Fetch();
    }

    public class BankHolidayFetcher : IBankHolidayFetcher
    {
        private readonly HttpClient client;

        public BankHolidayFetcher(HttpClient client) => this.client = client;

        public async Task<IReadOnlyList<LocalDate>> Fetch()
        {
            const string Url = "https://www.gov.uk/bank-holidays.json";

            var response = await this.client.GetAsync(Url);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<BankHolidayData>(responseContent)
                .EnglandAndWales
                .Events
                .Select(GetEventDate)
                .ToArray();
        }

        private static LocalDate GetEventDate(Event eventData) =>
            LocalDatePattern.Iso.Parse(eventData.Date).GetValueOrThrow();

        private class BankHolidayData
        {
            [JsonProperty(PropertyName = "england-and-wales")]
            public EnglandAndWales EnglandAndWales { get; set; }

            public Scotland Scotland { get; set; }

            [JsonProperty(PropertyName = "northern-ireland")]
            public NorthernIreland NorthernIreland { get; set; }
        }

        private class EnglandAndWales
        {
            public string Division { get; set; }

            public List<Event> Events { get; set; }
        }

        private class Scotland
        {
            public string Division { get; set; }

            public List<Event> Events { get; set; }
        }

        private class NorthernIreland
        {
            public string Division { get; set; }

            public List<Event> Events { get; set; }
        }

        private class Event
        {
            public string Title { get; set; }

            public string Date { get; set; }

            public string Notes { get; set; }

            public bool Bunting { get; set; }
        }
    }
}