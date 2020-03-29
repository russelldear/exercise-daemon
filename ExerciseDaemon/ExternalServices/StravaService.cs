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
using Newtonsoft.Json;
using static ExerciseDaemon.Constants.StatementSetKeys;

namespace ExerciseDaemon.ExternalServices
{
    public class StravaService
    {
        private const int DaysOfActivities = 7;

        private readonly StravaSettings _stravaSettings;
        private readonly AthleteRepository _athleteRepository;
        private readonly SlackService _slackService;
        private readonly StatementRandomiser _sr;
        private readonly HttpClient _client;

        public StravaService(StravaSettings stravaSettings, AthleteRepository athleteRepository, SlackService slackService, StatementRandomiser sr)
        {
            _stravaSettings = stravaSettings;
            _athleteRepository = athleteRepository;
            _slackService = slackService;
            _sr = sr;

            _client = new HttpClient();
        }

        public async Task<AthleteViewModel> GetOrCreateAthlete(TokenSet tokenSet, int athleteIdentifier, string name)
        {
            var existingAthlete = await _athleteRepository.GetAthlete(athleteIdentifier);

            if (existingAthlete == null)
            {
                tokenSet = await EnsureValidTokens(tokenSet, athleteIdentifier);

                return await CreateAthlete(tokenSet, athleteIdentifier, name);
            }
            else
            {
                tokenSet = await EnsureValidTokens(tokenSet, athleteIdentifier);

                return await GetAthlete(tokenSet, athleteIdentifier, name);
            }
        }

        private async Task<TokenSet> EnsureValidTokens(TokenSet tokenSet, int athleteIdentifier)
        {
            if (tokenSet.ExpiresAt.ToUniversalTime() < DateTimeOffset.UtcNow.AddMinutes(5))
            {
                var athlete = await _athleteRepository.GetAthlete(athleteIdentifier) ?? new Athlete();

                var url = $"https://www.strava.com/api/v3/oauth/token?client_id={_stravaSettings.ClientId}&client_secret={_stravaSettings.ClientSecret}&grant_type=refresh_token&refresh_token={tokenSet.RefreshToken}";

                var request = new HttpRequestMessage(HttpMethod.Post, url);

                var response = await _client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    tokenSet = JsonConvert.DeserializeObject<TokenSet>(responseString);

                    athlete.AccessToken = tokenSet.AccessToken;
                    athlete.RefreshToken = tokenSet.RefreshToken;
                    athlete.ExpiresAt = tokenSet.ExpiresAt;

                    await _athleteRepository.CreateOrUpdateAthlete(athlete);
                }
            }

            return tokenSet;
        }

        private async Task<AthleteViewModel> GetAthlete(TokenSet tokenSet, int athleteIdentifier, string name)
        {
            var activities = await GetRecentActivities(tokenSet, athleteIdentifier);

            var latestActivity = activities.FirstOrDefault();

            var athlete = new AthleteViewModel
            {
                Id = athleteIdentifier,
                Name = name,
                AccessToken = tokenSet.AccessToken,
                SignupDateTimeUtc = DateTime.UtcNow,
                ReminderCount = 0,
                LastReminderDateTimeUtc = DateTime.UtcNow,
                LatestActivityId = latestActivity?.Id,
                Activities = activities
            };

            return athlete;
        }

        private async Task<AthleteViewModel> CreateAthlete(TokenSet tokenSet, int athleteIdentifier, string name)
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
                SignupDateTimeUtc = DateTime.UtcNow,
                ReminderCount = 0,
                LastReminderDateTimeUtc = DateTime.UtcNow,
                LatestActivityId = latestActivity?.Id,
                Activities = activities
            };

            await _athleteRepository.CreateOrUpdateAthlete(athlete);

            var stravaAthlete = await GetStravaAthlete(tokenSet, athleteIdentifier);

            var welcomeString = $"Welcome, {stravaAthlete.FirstName} {stravaAthlete.LastName}! ";

            if (latestActivity != null)
            {
                var nowLocal = DateTime.UtcNow.AddSeconds(latestActivity.UtcOffset);

                if (latestActivity.StartDateLocal >= nowLocal.Date)
                {
                    welcomeString += string.Format(_sr.Get(WelcomeTodayPrompts), latestActivity.Type.ToLower());
                }
                else
                {
                    welcomeString += string.Format(_sr.Get(WelcomeRecentPrompts), latestActivity.Type.ToLower(), latestActivity.StartDateLocal.DayOfWeek);
                }
            }
            else
            {
                welcomeString += _sr.Get(WelcomeNeverPrompts);
            }

            await _slackService.PostSlackMessage(welcomeString);

            return athlete;
        }

        private async Task<StravaAthlete> GetStravaAthlete(TokenSet tokenSet, int athleteIdentifier)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://www.strava.com/api/v3/athlete");

            return await SendStravaRequest<StravaAthlete>(request, tokenSet, athleteIdentifier);
        }

        public async Task<List<Activity>> GetRecentActivities(TokenSet tokenSet, int athleteIdentifier)
        {
            tokenSet = await EnsureValidTokens(tokenSet, athleteIdentifier);

            var since = DateTime.UtcNow.AddDays(-DaysOfActivities).Date;

            var timestamp = (int)since.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://www.strava.com/api/v3/athlete/activities?after={timestamp}");
            
            var result = await SendStravaRequest<List<Activity>>(request, tokenSet, athleteIdentifier);

            return result.OrderByDescending(a => a.StartDateLocal).ToList();
        }

        private async Task<T> SendStravaRequest<T>(HttpRequestMessage request, TokenSet tokenSet, int athleteIdentifier)
        {
            tokenSet = await EnsureValidTokens(tokenSet, athleteIdentifier);

            T result = default(T);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenSet.AccessToken);

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                result = JsonConvert.DeserializeObject<T>(responseString);
            }

            return result;
        }
    }
}
