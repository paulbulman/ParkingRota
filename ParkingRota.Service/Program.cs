namespace ParkingRota.Service
{
    using System.ServiceProcess;

    public static class Program
    {
        private static void Main()
        {
            using (var service = new Service())
            {
                ServiceBase.Run(service);
            }
        }
    }
}