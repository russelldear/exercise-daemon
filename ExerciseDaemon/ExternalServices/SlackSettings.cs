namespace ExerciseDaemon.ExternalServices
{
    public class SlackSettings
    {
        public string SlackWebhookUrl { get; set; }

        public string SlackWorkspaceUrl { get; set; }

        public string SlackChannelId { get; set; }

        public string SlackBotUserAccessToken { get; set; }

        public string SlackClientId { get; set; }

        public string SlackClientSecret { get; set; }

        public string SlackRedirectUri { get; set; }
    }
}