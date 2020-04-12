using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExerciseDaemon.ExternalServices;
using ExerciseDaemon.Helpers;
using ExerciseDaemon.Models.Strava;
using ExerciseDaemon.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static ExerciseDaemon.Constants.StatementSetKeys;

namespace ExerciseDaemon.BackgroundWorker
{
    public class TimedBackgroundWorker : IHostedService, IDisposable
    {
        private const int FrequencySeconds = 300;

        private readonly StravaService _stravaService;
        private readonly AthleteRepository _athleteRepository;
        private readonly SlackService _slackService;
        private readonly MessageFactory _messageFactory;
        private readonly ILogger<TimedBackgroundWorker> _logger;
        private Timer _timer;

        public TimedBackgroundWorker(StravaService stravaService, AthleteRepository athleteRepository, SlackService slackService, MessageFactory messageFactory, ILogger<TimedBackgroundWorker> logger)
        {
            _stravaService = stravaService;
            _athleteRepository = athleteRepository;
            _slackService = slackService;
            _messageFactory = messageFactory;
            _logger = logger;
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
            _logger.LogInformation("Timed background worker starting.");
        
            try
            {
                var athletes = _athleteRepository.GetAthletes().Result;

                foreach (var athlete in athletes)
                {
                    var tokenSet = new StravaTokenSet(athlete.AccessToken, athlete.RefreshToken, athlete.ExpiresAt);

                    var activities = _stravaService.GetRecentActivities(tokenSet, athlete.Id).Result;

                    if (activities.Any())
                    {
                        CheckForNewActivity(athlete, activities);
                    }

                    if (athlete.ReminderCount == 0)
                    {
                        CheckForWeeklyReminder(activities, athlete);
                    }
                    else if (athlete.ReminderCount == 1)
                    {
                        CheckForFortnightReminder(athlete);
                    }
                    else if (athlete.ReminderCount == 2)
                    {
                        CheckForMonthReminder(athlete);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Timed background worker failed: '{e.Message}'");
            }
            
            _logger.LogInformation("Timed background worker ending.");
        }

        private void CheckForNewActivity(Athlete athlete, List<Activity> activities)
        {
            var hasNoRecordedActivities = !athlete.LatestActivityId.HasValue;

            var hasUnrecordedActivity = athlete.LatestActivityId.HasValue && athlete.LatestActivityId.Value != activities.First().Id;

            if (hasNoRecordedActivities || hasUnrecordedActivity)
            {
                var latestActivity = activities.First();

                athlete.LatestActivityId = latestActivity.Id;

                _athleteRepository.CreateOrUpdateAthlete(athlete).Wait();

                var message = _messageFactory.NewActivityMessage(athlete, latestActivity);

                _slackService.PostSlackMessage(message).Wait();
            }
        }

        private void CheckForWeeklyReminder(List<Activity> activities, Athlete athlete)
        {
            var dateToCheck = activities.FirstOrDefault()?.StartDate ?? athlete.SignupDateTimeUtc;

            if (dateToCheck < AWeekEarlier)
            {
                UpdateReminders(athlete, 1);

                _slackService.PostSlackMessage(_messageFactory.ReminderMessage(WeekReminder, athlete)).Wait();
            }
        }

        private void CheckForFortnightReminder(Athlete athlete)
        {
            if (athlete.LastReminderDateTimeUtc < AWeekEarlier)
            {
                UpdateReminders(athlete, 2);

                _slackService.PostSlackMessage(_messageFactory.ReminderMessage(FortnightReminder, athlete)).Wait();
            }
        }

        private void CheckForMonthReminder(Athlete athlete)
        {
            if (athlete.LastReminderDateTimeUtc < TwoWeeksEarlier)
            {
                UpdateReminders(athlete, 3);

                _slackService.PostSlackMessage(_messageFactory.ReminderMessage(MonthReminder, athlete)).Wait();
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
