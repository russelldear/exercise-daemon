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

            await client.PostAsync(_slackSettings.BaseUrl, body);
        }
    }
}
