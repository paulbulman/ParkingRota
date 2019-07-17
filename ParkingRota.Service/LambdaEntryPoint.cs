namespace ParkingRota.Service
{
    using System.Threading.Tasks;
    using Amazon.Lambda.Core;
    using Business;

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
            var secretsManager = new AwsSsmSecretsManager();

            var connectionString = await secretsManager.Fetch("/parkingrota/ParkingRotaConnectionString");

            return new TaskRunner(connectionString);
        }
    }
}