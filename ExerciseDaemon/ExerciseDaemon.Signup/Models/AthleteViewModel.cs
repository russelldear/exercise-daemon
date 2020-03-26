using System;
using System.Collections.Generic;
using ExerciseDaemon.Signup.Models.Strava;

namespace ExerciseDaemon.Signup.Models
{
    public class AthleteViewModel : Athlete
    {
        public string Name { get; set; }

        public DateTime StravaJoinDate { get; set; }

        public List<Activity> Activities { get; set; }
    }
}
