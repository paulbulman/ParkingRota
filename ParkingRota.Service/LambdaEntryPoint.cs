namespace ParkingRota.Service
{
    using System.Threading.Tasks;
    using Amazon.Lambda.Core;

    public class LambdaEntryPoint
    {
        private readonly Service service;

        public LambdaEntryPoint() => this.service = new Service();

        public async Task RunTasks(ILambdaContext context) => await this.service.RunTasksAsync();
    }
}