using System;

namespace ExerciseDaemon.Models.Strava
{
    public class Athlete
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string AccessToken { get; set; }

        public DateTime SignupDateTimeUtc { get; set; }

        public long? LatestActivityId { get; set; }
    }
}