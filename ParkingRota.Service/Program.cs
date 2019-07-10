namespace ParkingRota.Service
{
    using System.Diagnostics;
    using System.ServiceProcess;
    using System.Threading;
    using System.Threading.Tasks;
    using CommandLine;
    using NodaTime;

    public static class Program
    {
        private class Options
        {
            private const string RunOnceHelpText =
                "Whether the service loop should run just once and then exit, rather than run repeatedly.";

            private const string RunAsConsoleAppHelpText =
                "Whether the application should run as a normal console application, rather than as a Windows Service.";

            [Option('o', "runOnce", Required = false, HelpText = RunOnceHelpText)]
            public bool RunOnce { get; set; }

            [Option('c', "runAsConsoleApp", Required = false, HelpText = RunAsConsoleAppHelpText)]
            public bool RunAsConsoleApp { get; set; }
        }

        private static async Task Main(string[] args)
        {
            var runOnce = false;
            var runAsConsoleApp = false;

            Parser.Default
                .ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    runOnce = o.RunOnce;
                    runAsConsoleApp = o.RunAsConsoleApp;
                });

            using (var service = new Service())
            {
                if (runOnce)
                {
                    await service.RunTasksAsync();
                }
                else if (Debugger.IsAttached || runAsConsoleApp)
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