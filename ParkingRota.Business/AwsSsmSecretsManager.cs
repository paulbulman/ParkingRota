namespace ParkingRota.Business
{
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.SimpleSystemsManagement;
    using Amazon.SimpleSystemsManagement.Model;

    public class AwsSsmSecretsManager : ISecretsManager
    {
        public async Task<string> Fetch(string key)
        {
            using (var client = new AmazonSimpleSystemsManagementClient(RegionEndpoint.EUWest2))
            {
                var request = new GetParameterRequest { Name = key, WithDecryption = true };

                var response = await client.GetParameterAsync(request);

                return response.Parameter.Value;
            }
        }
    }
}