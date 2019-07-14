namespace ParkingRota
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;

    public class Program
    {
        public static void Main(string[] args)
        {
            SetAwsEnvironmentVariables();

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var builder = WebHost.CreateDefaultBuilder(args);

            if (Helpers.IsElasticBeanstalk())
            {
                builder.ConfigureAppConfiguration(configure => { configure.AddSystemsManager("/parkingrota/"); });
            }

            return builder.UseStartup<Startup>();
        }

        private static void SetAwsEnvironmentVariables()
        {
            const string ConfigurationFile = @"C:\Program Files\Amazon\ElasticBeanstalk\config\containerconfiguration";
            const string EnvironmentVariablesSectionKey = "iis:env";

            var configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.AddJsonFile(ConfigurationFile, optional: true, reloadOnChange: true);

            var environmentVariables =
                configurationBuilder
                    .Build()
                    .GetSection(EnvironmentVariablesSectionKey)
                    .GetChildren()
                    .Select(section => section.Value.Split('=', 2))
                    .ToDictionary(kvp => kvp[0], kvp => kvp[1]);

            foreach (var (variable, value) in environmentVariables)
            {
                Environment.SetEnvironmentVariable(variable, value);
            }
        }
    }
}
