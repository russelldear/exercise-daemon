using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ExerciseDaemon.Helpers;
using ExerciseDaemon.Models;
using ExerciseDaemon.Models.Strava;
using ExerciseDaemon.Repositories;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ExerciseDaemon.ExternalServices
{
    public class StravaService
    {
        private const int DaysOfActivities = 7;

        private readonly StravaSettings _stravaSettings;
        private readonly AthleteRepository _athleteRepository;
        private readonly SlackService _slackService;
        private readonly StatementRandomiser _sr;
        private readonly MessageFactory _messageFactory;
        private readonly ILogger<StravaService> _logger;
        private readonly HttpClient _client;

        public StravaService(StravaSettings stravaSettings, AthleteRepository athleteRepository, SlackService slackService, StatementRandomiser sr, MessageFactory messageFactory, ILogger<StravaService> logger)
        {
            _stravaSettings = stravaSettings;
            _athleteRepository = athleteRepository;
            _slackService = slackService;
            _sr = sr;
            _messageFactory = messageFactory;
            _logger = logger;
            _client = new HttpClient();
        }

        public async Task<AthleteViewModel> GetOrCreateAthlete(StravaTokenSet tokenSet, string slackUserId, int athleteIdentifier, string name)
        {
            var existingAthlete = await _athleteRepository.GetAthlete(athleteIdentifier);

            if (existingAthlete == null)
            {
                tokenSet = await EnsureValidTokens(tokenSet, athleteIdentifier);

                return await CreateAthlete(tokenSet, slackUserId, athleteIdentifier, name);
            }
            else
            {
                tokenSet = await EnsureValidTokens(tokenSet, athleteIdentifier);

                return await GetAthlete(tokenSet, slackUserId, athleteIdentifier, name);
            }
        }

        private async Task<StravaTokenSet> EnsureValidTokens(StravaTokenSet tokenSet, int athleteIdentifier)
        {
            _logger.LogInformation("Ensuring tokens.");

            if (tokenSet.ExpiresAt.ToUniversalTime() < DateTimeOffset.UtcNow.AddMinutes(5))
            {
                _logger.LogInformation("Token expiring; refreshing.");

                var athlete = await _athleteRepository.GetAthlete(athleteIdentifier) ?? new Athlete();

                var url = $"https://www.strava.com/api/v3/oauth/token?client_id={_stravaSettings.ClientId}&client_secret={_stravaSettings.ClientSecret}&grant_type=refresh_token&refresh_token={tokenSet.RefreshToken}";

                var request = new HttpRequestMessage(HttpMethod.Post, url);

                var response = await _client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    tokenSet = JsonConvert.DeserializeObject<StravaTokenSet>(responseString);

                    athlete.AccessToken = tokenSet.AccessToken;
                    athlete.RefreshToken = tokenSet.RefreshToken;
                    athlete.ExpiresAt = tokenSet.ExpiresAt;

                    _logger.LogInformation("Token refresh successful; updating saved athlete.");

                    await _athleteRepository.CreateOrUpdateAthlete(athlete);
                }
            }

            return tokenSet;
        }

        private async Task<AthleteViewModel> GetAthlete(StravaTokenSet tokenSet, string slackUserId, int athleteIdentifier, string name)
        {
            var athlete = await BuildAthleteViewModel(tokenSet, slackUserId, athleteIdentifier, name);

            return athlete;
        }

        private async Task<AthleteViewModel> CreateAthlete(StravaTokenSet tokenSet, string slackUserId, int athleteIdentifier, string name)
        {
            var athlete = await BuildAthleteViewModel(tokenSet, slackUserId, athleteIdentifier, name);

            var latestActivity = athlete.Activities.FirstOrDefault();

            await _athleteRepository.CreateOrUpdateAthlete(athlete);

            await PostWelcomeMessage(slackUserId, latestActivity);

            return athlete;
        }

        private async Task<AthleteViewModel> BuildAthleteViewModel(StravaTokenSet tokenSet, string slackUserId, int athleteIdentifier, string name)
        {
            var activities = await GetRecentActivities(tokenSet, athleteIdentifier);

            var latestActivity = activities.FirstOrDefault();

            var athlete = new AthleteViewModel
            {
                Id = athleteIdentifier,
                Name = name,
                AccessToken = tokenSet.AccessToken,
                RefreshToken = tokenSet.RefreshToken,
                ExpiresAt = tokenSet.ExpiresAt,
                SlackUserId = slackUserId,
                SignupDateTimeUtc = DateTime.UtcNow,
                ReminderCount = 0,
                LastReminderDateTimeUtc = DateTime.UtcNow,
                LatestActivityId = latestActivity?.Id,
                Activities = activities
            };
            return athlete;
        }

        public async Task<List<Activity>> GetRecentActivities(StravaTokenSet tokenSet, int athleteIdentifier)
        {
            tokenSet = await EnsureValidTokens(tokenSet, athleteIdentifier);

            var since = DateTime.UtcNow.AddDays(-DaysOfActivities).Date;

            var timestamp = (int)since.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://www.strava.com/api/v3/athlete/activities?after={timestamp}");
            
            var result = await SendStravaRequest<List<Activity>>(request, tokenSet, athleteIdentifier);

            return result.OrderByDescending(a => a.StartDateLocal).ToList();
        }

        private async Task<T> SendStravaRequest<T>(HttpRequestMessage request, StravaTokenSet tokenSet, int athleteIdentifier)
        {
            tokenSet = await EnsureValidTokens(tokenSet, athleteIdentifier);

            T result = default(T);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenSet.AccessToken);

            _logger.LogInformation($"Sending Strava request: {request.RequestUri}");

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Strava request successful.");

                var responseString = await response.Content.ReadAsStringAsync();

                result = JsonConvert.DeserializeObject<T>(responseString);
            }

            return result;
        }

        private async Task PostWelcomeMessage(string slackUserid, Activity latestActivity)
        {
            await _slackService.AddUserToChannel(slackUserid);

            var message = _messageFactory.WelcomeMessage(slackUserid, latestActivity);

            await _slackService.PostSlackMessage(message);
        }
    }
}
