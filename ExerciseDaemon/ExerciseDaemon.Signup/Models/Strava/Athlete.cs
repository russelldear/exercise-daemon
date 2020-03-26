using System;

namespace ExerciseDaemon.Signup.Models.Strava
{
    public class Athlete
    {
        public int Id { get; set; }

        public DateTime SignupDateTimeUtc { get; set; }

        public DateTime? LatestActivityDateTimeUtc { get; set; }
    }
}