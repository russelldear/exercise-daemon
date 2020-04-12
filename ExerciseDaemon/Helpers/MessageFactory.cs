﻿using System;
using System.Collections.Generic;
using System.Linq;
using ExerciseDaemon.ExternalServices;
using ExerciseDaemon.Models.Slack;
using ExerciseDaemon.Models.Strava;
using static ExerciseDaemon.Constants.StatementSetKeys;

namespace ExerciseDaemon.Helpers
{
    public class MessageFactory
    {
        private readonly StatementRandomiser _sr;
        private readonly GoogleMapsService _googleMaps;

        public MessageFactory(StatementRandomiser sr, GoogleMapsService googleMaps)
        {
            _sr = sr;
            _googleMaps = googleMaps;
        }

        public SlackMessage NewActivityMessage(Athlete athlete, Activity latestActivity)
        {
            var message = string.Format(_sr.Get(RecordNewActivity), GetSlackName(athlete), latestActivity.Type);

            string imageUrl = null;

            if (latestActivity.Map != null && !string.IsNullOrWhiteSpace(latestActivity.Map.SummaryPolyline))
            {
                imageUrl = _googleMaps.BuildMap(latestActivity.Id, latestActivity.Map.SummaryPolyline).Result;
            }

            var attachment = new Attachment{ ImageUrl = imageUrl };

            var fields = new List<Field>();

            var distance = latestActivity.Distance.ToFormattedDistance();

            if (!string.IsNullOrWhiteSpace(distance))
            {
                fields.Add(new Field { Title = "Distance", Value = distance, IsShort = true });
            }

            var elevation = latestActivity.Elevation.ToFormattedElevation();

            if (!string.IsNullOrWhiteSpace(elevation))
            {
                fields.Add(new Field { Title = "Elevation", Value = elevation, IsShort = true });
            }

            if (fields.Any())
            {
                attachment.Fields = fields.ToArray();
            }

            return new SlackMessage { Text = message, Attachments = new[] {attachment} };
        }

        public SlackMessage ReminderMessage(string reminderType, Athlete athlete)
        {
            var message = string.Format(_sr.Get(reminderType), GetSlackName(athlete));

            return new SlackMessage { Text = message };
        }

        public SlackMessage WelcomeMessage(string slackUserId, Activity latestActivity)
        {
            var welcomeString = $"Welcome, <@{slackUserId}>! ";

            if (latestActivity != null)
            {
                var nowLocal = DateTime.UtcNow.AddSeconds(latestActivity.UtcOffset);

                if (latestActivity.StartDateLocal >= nowLocal.Date)
                {
                    welcomeString += string.Format(_sr.Get(WelcomeTodayPrompts), latestActivity.Type.ToLower());
                }
                else
                {
                    welcomeString += string.Format(_sr.Get(WelcomeRecentPrompts), latestActivity.Type.ToLower(), latestActivity.StartDateLocal.DayOfWeek);
                }
            }
            else
            {
                welcomeString += _sr.Get(WelcomeNeverPrompts);
            }

            return new SlackMessage { Text = welcomeString };
        }

        private string GetSlackName(Athlete athlete)
        {
            return $"<@{athlete.SlackUserId}>";
        }
    }
}