using System;
using System.Threading;
using System.Threading.Tasks;
using ExerciseDaemon.Signup.ExternalServices;
using Microsoft.Extensions.Hosting;

namespace ExerciseDaemon.Signup.BackgroundWorker
{
    public class TimedBackgroundWorker : IHostedService, IDisposable
    {
        private readonly SlackService _slackService;
        private Timer _timer;

        public TimedBackgroundWorker(SlackService slackService)
        {
            _slackService = slackService;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(300));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            Console.WriteLine($"I ran: {DateTime.UtcNow:yyyy-MM-dd hh:mm:ss}");

            _slackService.PostSlackMessage("test").Wait();
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
