namespace ParkingRota.IntegrationTests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    public class ResponseHeadersMiddlewareTests : IClassFixture<ProductionWebApplicationFactory<Program>>
    {
        private readonly ProductionWebApplicationFactory<Program> factory;

        public ResponseHeadersMiddlewareTests(ProductionWebApplicationFactory<Program> factory) =>
            this.factory = factory;

        [Fact]
        public async Task Test_Headers()
        {
            // Arrange
            var client = this.factory.CreateClient();

            const string ExpectedContentSecurityPolicy =
                "default-src 'none'; " +
                "connect-src paulbulman.report-uri.com; " +
                "font-src 'self' cdnjs.cloudflare.com; " +
                "img-src 'self'; " +
                "script-src 'self' 'sha256-Ht5pieobFHQ7OBn1NV/L2c0mgYcW0/QdrzeaOpo0LWw=' cdnjs.cloudflare.com; " +
                "style-src 'self' 'unsafe-inline' cdnjs.cloudflare.com; " +
                "upgrade-insecure-requests; " +
                "report-uri https://paulbulman.report-uri.com/r/d/csp/enforce";

            const string ExpectedFeaturePolicy =
                "accelerometer 'none'; " +
                "camera 'none'; " +
                "geolocation 'none'; " +
                "gyroscope 'none'; " +
                "magnetometer 'none'; " +
                "microphone 'none'; " +
                "payment 'none'; " +
                "usb 'none'";

            // Act
            var response = await client.GetAsync("/");

            // Assert
            var expectedHeaders = new Dictionary<string, string>
            {
                { "Content-Security-Policy", ExpectedContentSecurityPolicy },
                { "X-Content-Type-Options", "nosniff" },
                { "Expect-CT", "max-age=0, report-uri=https://paulbulman.report-uri.com/r/d/ct/reportOnly" },
                { "Feature-Policy", ExpectedFeaturePolicy },
                { "x-frame-options", "DENY" },
                { "Referrer-Policy", "no-referrer" },
                { "X-Robots-Tag", "none" },
                { "X-Xss-Protection", "1; mode=block; report=https://paulbulman.report-uri.com/r/d/xss/enforce" }
            };

            foreach (var (key, value) in expectedHeaders)
            {
                Assert.True(response.Headers.Contains(key), $"Expected header '{key}' not present in HTTP response");

                Assert.Contains(value, response.Headers.GetValues(key));
            }
        }
    }
}