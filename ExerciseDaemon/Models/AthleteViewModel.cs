using System;
using System.Collections.Generic;
using ExerciseDaemon.Models.Strava;

namespace ExerciseDaemon.Models
{
    public class AthleteViewModel : Athlete
    {
        public DateTime StravaJoinDate { get; set; }

        public List<Activity> Activities { get; set; }

        public string SlackChannelUrl { get; set; }
    }
}
