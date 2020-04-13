using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExerciseDaemon.ExternalServices;
using ExerciseDaemon.Helpers;
using ExerciseDaemon.Models.Strava;
using ExerciseDaemon.Repositories;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static ExerciseDaemon.Constants.StatementSetKeys;

namespace ExerciseDaemon.BackgroundWorker
{
    public class DaemonTask
    {
        private readonly StravaService _stravaService;
        private readonly AthleteRepository _athleteRepository;
        private readonly SlackService _slackService;
        private readonly MessageFactory _messageFactory;
        private readonly ILogger<TimedBackgroundWorker> _logger;

        private DateTime AWeekEarlier => DateTime.UtcNow.AddDays(-7);

        private DateTime TwoWeeksEarlier => DateTime.UtcNow.AddDays(-14);

        public DaemonTask(StravaService stravaService, AthleteRepository athleteRepository, SlackService slackService, MessageFactory messageFactory, ILogger<TimedBackgroundWorker> logger)
        {
            _stravaService = stravaService;
            _athleteRepository = athleteRepository;
            _slackService = slackService;
            _messageFactory = messageFactory;
            _logger = logger;
        }

        public async Task Run()
        {
            _logger.LogInformation("Daemon task starting.");

            try
            {
                var athletes = await _athleteRepository.GetAthletes();

                _logger.LogInformation($"Athletes: {JsonConvert.SerializeObject(athletes)}");

                foreach (var athlete in athletes)
                {
                    _logger.LogInformation($"Checking {athlete.Name} now.");

                    var tokenSet = new StravaTokenSet(athlete.AccessToken, athlete.RefreshToken, athlete.ExpiresAt);

                    var activities = await _stravaService.GetRecentActivities(tokenSet, athlete.Id);

                    _logger.LogInformation($"Athlete: {JsonConvert.SerializeObject(athlete)}");
                    _logger.LogInformation($"Activities: {JsonConvert.SerializeObject(activities)}");

                    if (activities.Any())
                    {
                        await CheckForNewActivity(athlete, activities);
                    }
                    else
                    {
                        _logger.LogInformation($"No activities for {athlete.Name}.");
                    }

                    if (athlete.ReminderCount == 0)
                    {
                        await CheckForWeeklyReminder(activities, athlete);
                    }
                    else if (athlete.ReminderCount == 1)
                    {
                        await CheckForFortnightReminder(athlete);
                    }
                    else if (athlete.ReminderCount == 2)
                    {
                        await CheckForMonthReminder(athlete);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Daemon task failed: '{e.Message}'");

                await Task.Delay(20000); //Wait for logs to flush
            }

            _logger.LogInformation("Daemon task ending.");
        }

        private async Task CheckForNewActivity(Athlete athlete, List<Activity> activities)
        {
            var hasNoRecordedActivities = !athlete.LatestActivityId.HasValue;

            var hasUnrecordedActivity = athlete.LatestActivityId.HasValue && athlete.LatestActivityId.Value != activities.First().Id;

            if (hasNoRecordedActivities || hasUnrecordedActivity)
            {
                var latestActivity = activities.First();

                _logger.LogInformation($"Building message for {athlete.Name} now.");

                var message = await _messageFactory.NewActivityMessage(athlete, latestActivity);

                _logger.LogInformation($"Updating {athlete.Name} now.");

                athlete.LatestActivityId = latestActivity.Id;

                await _athleteRepository.CreateOrUpdateAthlete(athlete);

                _logger.LogInformation($"Posting message for {athlete.Name} now.");

                await _slackService.PostSlackMessage(message);

                _logger.LogInformation($"Posted for {athlete.Name}.");
            }
            else
            {
                _logger.LogInformation($"No new activity for {athlete.Name}.");
            }
        }

        private async Task CheckForWeeklyReminder(List<Activity> activities, Athlete athlete)
        {
            var dateToCheck = activities.FirstOrDefault()?.StartDate ?? athlete.SignupDateTimeUtc;

            if (dateToCheck < AWeekEarlier)
            {
                await UpdateReminders(athlete, 1);

                await _slackService.PostSlackMessage(_messageFactory.ReminderMessage(WeekReminder, athlete));
            }
        }

        private async Task CheckForFortnightReminder(Athlete athlete)
        {
            if (athlete.LastReminderDateTimeUtc < AWeekEarlier)
            {
                await UpdateReminders(athlete, 2);

                await _slackService.PostSlackMessage(_messageFactory.ReminderMessage(FortnightReminder, athlete));
            }
        }

        private async Task CheckForMonthReminder(Athlete athlete)
        {
            if (athlete.LastReminderDateTimeUtc < TwoWeeksEarlier)
            {
                await UpdateReminders(athlete, 3);

                await _slackService.PostSlackMessage(_messageFactory.ReminderMessage(MonthReminder, athlete));
            }
        }

        private async Task UpdateReminders(Athlete athlete, int reminderCount)
        {
            athlete.ReminderCount = reminderCount;
            athlete.LastReminderDateTimeUtc = DateTime.UtcNow;

            await _athleteRepository.CreateOrUpdateAthlete(athlete);
        }
    }
}
