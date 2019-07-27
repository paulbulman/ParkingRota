namespace ParkingRota.Monitor
{
    using System;
    using System.Threading.Tasks;
    using Amazon.Lambda.Core;
    using Business;
    using Business.EmailSenders;
    using Business.EmailTemplates;
    using Business.Model;
    using NodaTime;

    public class Program
    {
        private LastServiceRunTimeFetcher lastServiceRunTimeFetcher;
        private AwsSesEmailSender emailSender;
        private IClock clock;

        private Instant? lastUnhealthyTime;
        private Exception lastException;

        private static string SmtpTo => Environment.GetEnvironmentVariable("SmtpTo");

        public async Task CheckLastServiceRunTime(ILambdaContext context)
        {
            await this.EnsureDependenciesCreated();

            try
            {
                var lastServiceRunTime = GetLastServiceRunTime();

                var timeSinceLastServiceRun = this.clock.GetCurrentInstant().Minus(lastServiceRunTime);

                var serviceIsHealthy = timeSinceLastServiceRun.TotalMinutes < 5;

                if (serviceIsHealthy)
                {
                    Console.WriteLine("Service is healthy");

                    await this.HandleServiceHealthy(lastServiceRunTime);
                }
                else
                {
                    Console.WriteLine("Service is unhealthy");

                    await this.HandleServiceUnhealthy(lastServiceRunTime);
                }

                this.lastException = null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception caught: {e.Message}");

                await this.HandleException(e);
            }
        }

        private Instant GetLastServiceRunTime()
        {
            var lastServiceRunTime = Instant.MinValue;

            var task = Task.Run(() => lastServiceRunTime = this.lastServiceRunTimeFetcher.Fetch());

            var timeout = (int)Duration.FromSeconds(10).TotalMilliseconds;

            if (task.Wait(timeout))
            {
                return lastServiceRunTime;
            }

            throw new TimeoutException("Timed out fetching last service run time from database");
        }

        private async Task HandleServiceHealthy(Instant lastServiceRunTime)
        {
            var problemResolved = this.lastUnhealthyTime != null || this.lastException != null;

            if (problemResolved)
            {
                await SendEmail(new ServiceProblemResolved(SmtpTo, lastServiceRunTime));
            }

            this.lastUnhealthyTime = null;
            this.lastException = null;
        }

        private async Task HandleServiceUnhealthy(Instant lastServiceRunTime)
        {
            var problemIsNew = this.lastUnhealthyTime == null || lastServiceRunTime > this.lastUnhealthyTime.Value;

            if (problemIsNew)
            {
                Console.WriteLine("Problem is new");

                this.lastUnhealthyTime = lastServiceRunTime;

                await SendEmail(new ServiceProblemWarning(SmtpTo, lastServiceRunTime));
            }
        }

        private async Task HandleException(Exception exception)
        {
            var exceptionIsNew = this.lastException == null;

            if (exceptionIsNew)
            {
                Console.WriteLine("Exception is new");

                this.lastException = exception;

                await SendEmail(new ServiceMonitorExceptionWarning(SmtpTo, exception));
            }
        }

        private async Task SendEmail(IEmailTemplate email)
        {
            try
            {
                Console.WriteLine("Attempting to send email");

                await this.emailSender.Send(email);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception attempting to send email: {e.Message}");
            }
        }

        private async Task EnsureDependenciesCreated()
        {
            var secretsManager = new AwsSsmSecretsManager();

            if (this.lastServiceRunTimeFetcher == null)
            {
                var connectionString = await secretsManager.Fetch("/parkingrota/ParkingRotaMonitorConnectionString");

                this.lastServiceRunTimeFetcher = new LastServiceRunTimeFetcher(connectionString);
            }

            if (this.emailSender == null)
            {
                this.emailSender = new AwsSesEmailSender(secretsManager, new MonitorSystemParameterListRepository());
            }

            if (this.clock == null)
            {
                this.clock = SystemClock.Instance;
            }
        }

        private class MonitorSystemParameterListRepository : ISystemParameterListRepository
        {
            public SystemParameterList GetSystemParameterList() =>
                new SystemParameterList { FromEmailAddress = Environment.GetEnvironmentVariable("SmtpFrom") };

            public void UpdateSystemParameterList(SystemParameterList updated) => throw new NotImplementedException();
        }
    }
}
