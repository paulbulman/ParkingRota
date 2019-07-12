namespace ParkingRota.Service
{
    using System;
    using System.ServiceProcess;
    using System.Threading;
    using System.Timers;
    using NodaTime;
    using Timer = System.Timers.Timer;

    public class Service : ServiceBase
    {
        private readonly TaskRunner taskRunner;
        private readonly Timer timer;

        public Service()
        {
            this.taskRunner = new TaskRunner();
            this.timer = new Timer(Duration.FromMinutes(1).TotalMilliseconds);
        }

        public void Start(Duration timerInterval)
        {
            this.timer.Interval = timerInterval.TotalMilliseconds;

            this.OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            this.timer.Elapsed += this.Timer_Elapsed;
            this.timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (Monitor.TryEnter(this))
                {
                    try
                    {
                        this.RunTasks();
                    }
                    finally
                    {
                        Monitor.Exit(this);
                    }
                }
            }
            catch (Exception exception)
            {
                // Exceptions get swallowed by the caller of this, so we need to make sure we don't ignore them.
                ThreadPool.QueueUserWorkItem(
                    callback => throw new InvalidOperationException("Timer process exception", exception));
            }
        }

        protected override void OnStop()
        {
            this.timer.Stop();

            lock (this)
            {
                // Ensure any current run has finished
            }

            base.OnStop();
            this.Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            this.taskRunner.Dispose();
            this.timer.Dispose();
        }

        private async void RunTasks() => await this.taskRunner.RunTasksAsync();
    }
}
