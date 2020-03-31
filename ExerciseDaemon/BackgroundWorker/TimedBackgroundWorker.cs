using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExerciseDaemon.ExternalServices;
using ExerciseDaemon.Helpers;
using ExerciseDaemon.Models.Strava;
using ExerciseDaemon.Repositories;
using Microsoft.Extensions.Hosting;
using static ExerciseDaemon.Constants.StatementSetKeys;

namespace ExerciseDaemon.BackgroundWorker
{
    public class TimedBackgroundWorker : IHostedService, IDisposable
    {
        private const int FrequencySeconds = 300;

        private readonly StravaService _stravaService;
        private readonly AthleteRepository _athleteRepository;
        private readonly SlackService _slackService;
        private readonly StatementRandomiser _sr;
        private Timer _timer;

        public TimedBackgroundWorker(StravaService stravaService, AthleteRepository athleteRepository, SlackService slackService, StatementRandomiser sr)
        {
            _stravaService = stravaService;
            _athleteRepository = athleteRepository;
            _slackService = slackService;
            _sr = sr;
        }

        private DateTime AWeekEarlier => DateTime.UtcNow.AddDays(-7);
        
        private DateTime TwoWeeksEarlier => DateTime.UtcNow.AddDays(-14);

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
                var tokenSet = new StravaTokenSet(athlete.AccessToken, athlete.RefreshToken, athlete.ExpiresAt);

                var activities = _stravaService.GetRecentActivities(tokenSet, athlete.Id).Result;

                if (activities.Any())
                {
                    var hasNoRecordedActivities = !athlete.LatestActivityId.HasValue;

                    var hasUnrecordedActivity = athlete.LatestActivityId.HasValue && athlete.LatestActivityId.Value != activities.First().Id;

                    if (hasNoRecordedActivities || hasUnrecordedActivity)
                    {
                        var latestActivity = activities.First();

                        athlete.LatestActivityId = latestActivity.Id;

                        _athleteRepository.CreateOrUpdateAthlete(athlete).Wait();

                        _slackService.PostSlackMessage(string.Format(_sr.Get(RecordNewActivity), athlete.Name, latestActivity.Type)).Wait();
                    }
                }

                if (athlete.ReminderCount == 0)
                {
                    var dateToCheck = activities.FirstOrDefault()?.StartDate ?? athlete.SignupDateTimeUtc;

                    if (dateToCheck < AWeekEarlier)
                    {
                        UpdateReminders(athlete, 1);

                        _slackService.PostSlackMessage(string.Format(_sr.Get(WeekReminder), athlete.Name)).Wait();
                    }
                }
                else if (athlete.ReminderCount == 1)
                {
                    if (athlete.LastReminderDateTimeUtc < AWeekEarlier)
                    {
                        UpdateReminders(athlete, 2);

                        _slackService.PostSlackMessage(string.Format(_sr.Get(FortnightReminder), athlete.Name)).Wait();
                    }
                }
                else if (athlete.ReminderCount == 2)
                {
                    if (athlete.LastReminderDateTimeUtc < TwoWeeksEarlier)
                    {
                        UpdateReminders(athlete, 3);

                        _slackService.PostSlackMessage(string.Format(_sr.Get(MonthReminder), athlete.Name)).Wait();
                    }
                }
            }
        }

        private void UpdateReminders(Athlete athlete, int reminderCount)
        {
            athlete.ReminderCount = reminderCount;
            athlete.LastReminderDateTimeUtc = DateTime.UtcNow;

            _athleteRepository.CreateOrUpdateAthlete(athlete).Wait();
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
