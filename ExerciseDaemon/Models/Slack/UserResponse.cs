namespace ExerciseDaemon.Models.Slack
{
    public class UserResponse
    {
        public bool ok { get; set; }

        public User user { get; set; }

        public Team team { get; set; }
    }
}