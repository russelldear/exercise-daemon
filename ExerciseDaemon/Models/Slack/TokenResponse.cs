namespace ExerciseDaemon.Models.Slack
{
    public class TokenResponse
    {
        public bool ok { get; set; }

        public string access_token { get; set; }

        public string scope { get; set; }

        public string team_id { get; set; }
    }
}