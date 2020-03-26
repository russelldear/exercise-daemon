using System;
using Newtonsoft.Json;

namespace ExerciseDaemon.Models.Strava
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

        [JsonProperty("start_date")]
        public DateTime StartDate { get; set; }

        [JsonProperty("start_date_local")]
        public DateTime StartDateLocal { get; set; }

        public string Timezone { get; set; }

        [JsonProperty("utc_offset")]
        public double UtcOffset { get; set; }

        [JsonProperty("suffer_score")]
        public int SufferScore { get; set; }
    }
}