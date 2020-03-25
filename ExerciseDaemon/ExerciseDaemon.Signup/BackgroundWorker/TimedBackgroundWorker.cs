using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace ExerciseDaemon.Signup.BackgroundWorker
{
    public class TimedBackgroundWorker : IHostedService, IDisposable
    {
        private Timer _timer;

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(300));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            Console.WriteLine($"I ran: {DateTime.UtcNow:yyyy-MM-dd hh:mm:ss}");
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
