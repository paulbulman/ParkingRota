namespace ParkingRota.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AngleSharp.Dom;
    using AngleSharp.Dom.Html;
    using Xunit;
    using HttpMethod = System.Net.Http.HttpMethod;

    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> SendAsync(
            this HttpClient client, IHtmlFormElement form, IHtmlElement submitButton) =>
                client.SendAsync(form, submitButton, new Dictionary<string, string>());

        public static Task<HttpResponseMessage> SendAsync(
            this HttpClient client, IHtmlFormElement form, IEnumerable<KeyValuePair<string, string>> formValues) =>
                client.SendAsync(form, GetSubmitButton(form), formValues);

        public static Task<HttpResponseMessage> SendAsync(
            this HttpClient client,
            IHtmlFormElement form,
            IHtmlElement submitButton,
            IEnumerable<KeyValuePair<string, string>> formValues)
        {
            foreach (var formValue in formValues)
            {
                var element = Assert.IsAssignableFrom<IHtmlInputElement>(form[formValue.Key]);

                element.Value = formValue.Value;
            }

            var documentRequest = form.GetSubmission(submitButton);

            var requestUri = submitButton.HasAttribute("formaction") ?
                new Uri(submitButton.GetAttribute("formaction"), UriKind.Relative) :
                documentRequest.Target;

            var request = new HttpRequestMessage(new HttpMethod(documentRequest.Method.ToString()), requestUri)
            {
                Content = new StreamContent(documentRequest.Body)
            };

            foreach (var header in documentRequest.Headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return client.SendAsync(request);
        }

        private static IHtmlElement GetSubmitButton(IParentNode form) =>
            Assert.IsAssignableFrom<IHtmlElement>(
                Assert.Single(form.QuerySelectorAll("[type=submit]")));
    }
}
