namespace ParkingRota.Service
{
    using System.Threading.Tasks;
    using Amazon.Lambda.Core;

    public class LambdaEntryPoint
    {
        private readonly TaskRunner taskRunner;

        public LambdaEntryPoint() => this.taskRunner = new TaskRunner();

        public async Task RunTasks(ILambdaContext context) => await this.taskRunner.RunTasksAsync();
    }
}