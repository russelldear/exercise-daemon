using System;
using Newtonsoft.Json;

namespace ExerciseDaemon.Signup.Models.Strava
{
    public class Activity
    {
        public long Id { get; set; }

        public Athlete Athlete { get; set; }

        public string Name { get; set; }
        
        public string Type { get; set; }

        [JsonProperty("private")]
        public bool IsPrivate { get; set; }

        public float Distance { get; set; }
        public DateTime StartDate { get; set; }

        public DateTime StartDateLocal { get; set; }

        public string Timezone { get; set; }

        public int UtcOffset { get; set; }

        public int SufferScore { get; set; }
    }
}