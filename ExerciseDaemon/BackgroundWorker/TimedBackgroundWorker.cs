using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExerciseDaemon.ExternalServices;
using ExerciseDaemon.Repositories;
using Microsoft.Extensions.Hosting;

namespace ExerciseDaemon.BackgroundWorker
{
    public class TimedBackgroundWorker : IHostedService, IDisposable
    {
        private const int FrequencySeconds = 300;

        private readonly StravaService _stravaService;
        private readonly AthleteRepository _athleteRepository;
        private readonly SlackService _slackService;
        private Timer _timer;

        public TimedBackgroundWorker(StravaService stravaService, AthleteRepository athleteRepository, SlackService slackService)
        {
            _stravaService = stravaService;
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
            var athletes = _athleteRepository.GetAthletes().Result;

            foreach (var athlete in athletes)
            {
                var activities = _stravaService.GetRecentActivities(athlete.AccessToken).Result;

                if (activities.Any())
                {
                    var hasNoRecordedActivities = !athlete.LatestActivityId.HasValue;

                    var hasUnrecordedActivity = athlete.LatestActivityId.HasValue && athlete.LatestActivityId.Value != activities.First().Id;

                    if (hasNoRecordedActivities || hasUnrecordedActivity)
                    {
                        var latestActivity = activities.First();

                        athlete.LatestActivityId = latestActivity.Id;

                        _athleteRepository.CreateOrUpdateAthlete(athlete).Wait();

                        _slackService.PostSlackMessage($"{athlete.Name} just completed a {latestActivity.Type}. Nice work!").Wait();
                    }
                }
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
