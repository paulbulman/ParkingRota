namespace ParkingRota.Service
{
    using System.Diagnostics;
    using System.ServiceProcess;
    using System.Threading;
    using NodaTime;

    public static class Program
    {
        private static void Main()
        {
            using (var service = new Service())
            {
                if (Debugger.IsAttached)
                {
                    service.Start(Duration.FromSeconds(10));
                    Thread.Sleep(-1);
                }
                else
                {
                    ServiceBase.Run(service);
                }
            }
        }
    }
}