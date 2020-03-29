using System;

namespace ExerciseDaemon.Models.Strava
{
    public class Athlete
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime SignupDateTimeUtc { get; set; }

        public int ReminderCount { get; set; }

        public DateTime LastReminderDateTimeUtc { get; set; }

        public long? LatestActivityId { get; set; }
    }
}