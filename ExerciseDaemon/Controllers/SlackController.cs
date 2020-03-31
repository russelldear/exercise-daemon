using System;
using System.Net.Http;
using System.Threading.Tasks;
using ExerciseDaemon.ExternalServices;
using ExerciseDaemon.Models.Slack;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ExerciseDaemon.Controllers
{
    public class SlackController : Controller
    {
        private const string AuthorizeUrlFormat = "https://slack.com/oauth/authorize?scope=identity.basic&client_id={0}&redirect_uri={1}";
        private const string TokenUrlFormat = "https://slack.com/api/oauth.access?client_id={0}&client_secret={1}&code={2}&redirect_uri={3}";
        private const string UserUrlFormat = "https://slack.com/api/users.identity?token={0}";


        private readonly SlackSettings _slackSettings;
        private readonly HttpClient _client;

        public SlackController(SlackSettings slackSettings)
        {
            _slackSettings = slackSettings;

            _client = new HttpClient();
        }

        public IActionResult Connect()
        {
            return Redirect(string.Format(AuthorizeUrlFormat, _slackSettings.ClientId, _slackSettings.RedirectUri));
        }

        public async Task<IActionResult> Connected([FromQuery] string code)
        {
            try
            {
                var tokenSet = await GetToken(code);

                var userResponse = await GetUser(tokenSet.access_token);

                Response.Cookies.Append("SlackUserid", userResponse.user.id);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task<TokenResponse> GetToken(string code)
        {
            var url = string.Format(TokenUrlFormat, _slackSettings.ClientId, _slackSettings.ClientSecret, code, _slackSettings.RedirectUri);

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());
            }

            return null;
        }

        private async Task<UserResponse> GetUser(string token)
        {
            var userUrl = string.Format(UserUrlFormat, token);

            var userRequest = new HttpRequestMessage(HttpMethod.Get, userUrl);

            var response = await _client.SendAsync(userRequest);

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<UserResponse>(await response.Content.ReadAsStringAsync());
            }

            return null;
        }

        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}