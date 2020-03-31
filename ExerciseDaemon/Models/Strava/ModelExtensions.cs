using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DocumentModel;
using static ExerciseDaemon.Constants.DocumentProperties;

namespace ExerciseDaemon.Models.Strava
{
    public static class ModelExtensions
    {
        public static Athlete ToAthlete(this Document document)
        {
            long? latestActivityId = null;

            if (!(document[LatestActivityId] is DynamoDBNull))
            {
                latestActivityId = long.Parse(document[LatestActivityId]);
            }

            return new Athlete
            {
                Id = int.Parse(document[Id]),
                Name = document[Name].ToString(),
                AccessToken = document[AccessToken].ToString(),
                RefreshToken = document[RefreshToken].ToString(),
                ExpiresAt = DateTime.Parse(document[ExpiresAt]).ToUniversalTime(),
                SlackUserId = document[SlackUserId].ToString(),
                SignupDateTimeUtc = DateTime.Parse(document[SignupDateTimeUtc]).ToUniversalTime(),
                ReminderCount = int.Parse(document[ReminderCount]),
                LastReminderDateTimeUtc = DateTime.Parse(document[LastReminderDateTimeUtc]).ToUniversalTime(),
                LatestActivityId = latestActivityId
            };
        }

        public static List<Athlete> ToAthletes(this List<Document> documents)
        {
            var result = new List<Athlete>();

            foreach (var document in documents)
            {
                result.Add(document.ToAthlete());
            }

            return result;
        }
    }
}