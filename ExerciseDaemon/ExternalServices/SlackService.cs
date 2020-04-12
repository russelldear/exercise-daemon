using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ExerciseDaemon.Models.Slack;
using Newtonsoft.Json;

namespace ExerciseDaemon.ExternalServices
{
    public class SlackService
    {
        private const string AddChannelFormat = "https://slack.com/api/conversations.invite?token={0}&channel={1}&users={2}";

        private readonly SlackSettings _slackSettings;
        private readonly HttpClient _client;

        public SlackService(SlackSettings slackSettings)
        {
            _slackSettings = slackSettings;

            _client = new HttpClient();
        }

        public async Task PostSlackMessage(SlackMessage slackMessage)
        {
            var bodyString = JsonConvert.SerializeObject(slackMessage);

            var body = new StringContent(bodyString, Encoding.UTF8, "application/json");

            await _client.PostAsync(_slackSettings.SlackWebhookUrl, body);
        }

        public async Task<HttpResponseMessage> AddUserToChannel(string slackUserId)
        {
            var url = string.Format(AddChannelFormat, _slackSettings.SlackBotUserAccessToken, _slackSettings.SlackChannelId, slackUserId);

            var request = new HttpRequestMessage(HttpMethod.Post, url);

            return await _client.SendAsync(request);
        }
    }

}
