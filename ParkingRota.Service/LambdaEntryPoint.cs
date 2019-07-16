namespace ParkingRota.Service
{
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.Lambda.Core;
    using Amazon.SimpleSystemsManagement;
    using Amazon.SimpleSystemsManagement.Model;

    public class LambdaEntryPoint
    {
        private TaskRunner taskRunner;

        public async Task RunTasks(ILambdaContext context)
        {
            if (this.taskRunner == null)
            {
                this.taskRunner = await CreateTaskRunner();
            }

            await this.taskRunner.RunTasksAsync();
        }

        private static async Task<TaskRunner> CreateTaskRunner()
        {
            using (var client = new AmazonSimpleSystemsManagementClient(RegionEndpoint.EUWest2))
            {
                var request = new GetParameterRequest
                {
                    Name = "/parkingrota/ParkingRotaConnectionString",
                    WithDecryption = true
                };

                var response = await client.GetParameterAsync(request);

                var connectionString = response.Parameter.Value;

                return new TaskRunner(connectionString);
            }
        }
    }
}