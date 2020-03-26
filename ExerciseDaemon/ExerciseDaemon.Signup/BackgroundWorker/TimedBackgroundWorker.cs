using System;
using System.Threading;
using System.Threading.Tasks;
using ExerciseDaemon.Signup.ExternalServices;
using ExerciseDaemon.Signup.Repositories;
using Microsoft.Extensions.Hosting;

namespace ExerciseDaemon.Signup.BackgroundWorker
{
    public class TimedBackgroundWorker : IHostedService, IDisposable
    {
        private const int FrequencySeconds = 300;

        private readonly AthleteRepository _athleteRepository;
        private readonly SlackService _slackService;
        private Timer _timer;

        public TimedBackgroundWorker(AthleteRepository athleteRepository, SlackService slackService)
        {
            _athleteRepository = athleteRepository;
            _slackService = slackService;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(FrequencySeconds));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            Console.WriteLine($"Background worker process ran: {DateTime.UtcNow:yyyy-MM-dd hh:mm:ss}");

            var lastRan = DateTime.UtcNow.AddSeconds(-FrequencySeconds);

            var athletes = _athleteRepository.GetAthletes().Result;

            foreach (var athlete in athletes)
            {
                if (athlete.LatestActivityDateTimeUtc.HasValue && athlete.LatestActivityDateTimeUtc.Value > lastRan)
                { }
            }
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
