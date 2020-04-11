using Newtonsoft.Json;

namespace ExerciseDaemon.Models.Strava
{
    public class Map
    {
        public string Id { get; set; }

        [JsonProperty("summary_polyline")]
        public string SummaryPolyline { get; set; }
    }
}