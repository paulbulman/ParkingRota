namespace ParkingRota.Business
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public interface IPasswordBreachChecker
    {
        Task<bool> PasswordIsBreached(string password);
    }

    public class PasswordBreachChecker : IPasswordBreachChecker
    {
        private readonly ILogger<PasswordBreachChecker> logger;
        private readonly HttpClient client;

        public PasswordBreachChecker(ILogger<PasswordBreachChecker> logger, HttpClient client)
        {
            this.logger = logger;
            this.client = client;
        }

        public async Task<bool> PasswordIsBreached(string password)
        {
            const string ErrorMessage =
                "An error occurred checking whether the specified password was breached. Assuming password is okay.";

            var hash = ComputeHash(password);

            var hashPrefix = hash.Substring(0, 5);
            var hashSuffix = hash.Substring(5);

            try
            {
                var response = await this.CallApi(hashPrefix);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return ResponseContainsHashSuffix(responseContent, hashSuffix);
                }

                this.logger.LogError(ErrorMessage + $"\r\nThe response from the API was:\r\n{responseContent}");
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception, ErrorMessage);
            }

            return false;
        }

        private static string ComputeHash(string password) =>
            string.Join(
                string.Empty,
                SHA1.Create()
                    .ComputeHash(Encoding.UTF8.GetBytes(password))
                    .Select(b => b.ToString("X2")));

        private async Task<HttpResponseMessage> CallApi(string hashPrefix) =>
            await this.client.GetAsync($"https://api.pwnedpasswords.com/range/{hashPrefix}");

        private static bool ResponseContainsHashSuffix(string responseContent, string hashSuffix) =>
            responseContent
                .Split("\r\n")
                .Select(record => record.Split(":").First())
                .Any(suffix => string.Equals(suffix, hashSuffix, StringComparison.InvariantCultureIgnoreCase));
    }
}