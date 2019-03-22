namespace ParkingRota.IntegrationTests
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using AngleSharp;
    using AngleSharp.Html.Dom;
    using AngleSharp.Io;

    public static class HtmlHelpers
    {
        public static async Task<IHtmlDocument> GetDocumentAsync(HttpResponseMessage response) =>
            (IHtmlDocument)await BrowsingContext
                .New()
                .OpenAsync(virtualResponse => ResponseFactory(virtualResponse, response));

        private static async void ResponseFactory(VirtualResponse virtualResponse, HttpResponseMessage response)
        {
            virtualResponse
                .Address(response.RequestMessage.RequestUri)
                .Status(response.StatusCode)
                .Content(await response.Content.ReadAsStringAsync());

            MapHeaders(virtualResponse, response.Headers);
            MapHeaders(virtualResponse, response.Content.Headers);
        }

        private static void MapHeaders(VirtualResponse virtualResponse, HttpHeaders headers)
        {
            foreach (var header in headers)
            {
                foreach (var value in header.Value)
                {
                    virtualResponse.Header(header.Key, value);
                }
            }
        }
    }
}
