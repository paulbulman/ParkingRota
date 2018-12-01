namespace ParkingRota.Service
{
    using System;
    using Amazon.Lambda.Core;
    using Microsoft.Extensions.DependencyInjection;
    using NodaTime;

    public class Program
    {
        public void RunTasks(ILambdaContext context)
        {
            var services = new ServiceCollection();

            services.AddSingleton<IClock>(SystemClock.Instance);

            var serviceScopeFactory = services
                .BuildServiceProvider()
                .GetRequiredService<IServiceScopeFactory>();

            using (var scope = serviceScopeFactory.CreateScope())
            {
                var clock = scope.ServiceProvider.GetRequiredService<IClock>();

                Console.WriteLine($"Service running at {clock.GetCurrentInstant()}");
            }
        }
    }
}
