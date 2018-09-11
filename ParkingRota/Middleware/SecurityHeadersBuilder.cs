namespace ParkingRota.Middleware
{
    public class SecurityHeadersBuilder
    {
        private const string ReportUriSubdomain = "paulbulman.report-uri.com";
        private const string CloudflareSubdomain = "cdnjs.cloudflare.com";

        private readonly SecurityHeadersPolicy policy;

        public SecurityHeadersBuilder() => this.policy = new SecurityHeadersPolicy();

        public SecurityHeadersPolicy Build() => this.policy;

        public SecurityHeadersBuilder AddSecurityHeaders()
        {
            this.AddContentSecurityPolicy();
            this.AddContentTypeOptions();
            this.AddExpectCertificateTransparency();
            this.AddFeaturePolicy();
            this.AddFrameOptions();
            this.AddReferrerPolicy();
            this.AddXssProtection();

            return this;
        }

        private void AddContentSecurityPolicy() =>
            this.policy.AddHeader(
                "Content-Security-Policy",
                "default-src 'none'; " +
                $"connect-src {ReportUriSubdomain}; " +
                $"font-src 'self' {CloudflareSubdomain}; " +
                "img-src 'self'; " +
                $"script-src 'self' 'sha256-Ht5pieobFHQ7OBn1NV/L2c0mgYcW0/QdrzeaOpo0LWw=' {CloudflareSubdomain}; " +
                $"style-src 'self' 'unsafe-inline' {CloudflareSubdomain}; " +
                "upgrade-insecure-requests; " +
                $"report-uri https://{ReportUriSubdomain}/r/d/csp/enforce");

        private void AddContentTypeOptions() => this.policy.AddHeader("X-Content-Type-Options", "nosniff");

        private void AddExpectCertificateTransparency() =>
            this.policy.AddHeader(
                "Expect-CT",
                $"max-age=0, report-uri=https://{ReportUriSubdomain}/r/d/ct/reportOnly");

        private void AddFeaturePolicy() =>
            this.policy.AddHeader(
                "Feature-Policy",
                "accelerometer 'none'; " +
                "camera 'none'; " +
                "geolocation 'none'; " +
                "gyroscope 'none'; " +
                "magnetometer 'none'; " +
                "microphone 'none'; " +
                "payment 'none'; " +
                "usb 'none'");

        private void AddFrameOptions() => this.policy.AddHeader("x-frame-options", "DENY");

        private void AddReferrerPolicy() => this.policy.AddHeader("Referrer-Policy", "no-referrer");

        private void AddXssProtection() =>
            this.policy.AddHeader(
                "X-Xss-Protection",
                $"1; mode=block; report=https://{ReportUriSubdomain}/r/d/xss/enforce");
    }
}