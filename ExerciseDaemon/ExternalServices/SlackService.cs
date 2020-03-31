using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ExerciseDaemon.ExternalServices
{
    public class SlackService
    {
        private readonly SlackSettings _slackSettings;

        public SlackService(SlackSettings slackSettings)
        {
            _slackSettings = slackSettings;
        }

        public async Task PostSlackMessage(string message)
        {
            var client = new HttpClient();

            var bodyString = $"{{\"text\": \"{message}\"}}";

            var body = new StringContent(bodyString, Encoding.UTF8, "application/json");

            await client.PostAsync(_slackSettings.SlackWebhookUrl, body);
        }

        public Task AddUserToChannel(string slackUserid)
        {
            return null;
            //https://slack.com/api/conversations.invite?token=xoxb-1034559977669-1036517533078-GuMykwaKhQ0DJRYV95UE5Sa1&channel=C01150C5VBN&users=U0117FCK687
        }
    }
}
