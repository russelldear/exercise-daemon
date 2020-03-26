using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ExerciseDaemon.Models;
using ExerciseDaemon.Models.Strava;
using ExerciseDaemon.Repositories;
using Newtonsoft.Json;

namespace ExerciseDaemon.ExternalServices
{
    public class StravaService
    {
        private const int DaysOfActivities = 7;

        private readonly AthleteRepository _athleteRepository;
        private readonly SlackService _slackService;
        private readonly HttpClient _client;

        public StravaService(AthleteRepository athleteRepository, SlackService slackService)
        {
            _athleteRepository = athleteRepository;
            _slackService = slackService;

            _client = new HttpClient();
        }

        public async Task<AthleteViewModel> GetOrCreateAthlete(string accessToken, int athleteIdentifier, string name)
        {
            var activities = await GetRecentActivities(accessToken);

            var latestActivity = activities.OrderByDescending(a => a.StartDateLocal).FirstOrDefault();

            var athlete = new AthleteViewModel
            {
                Id = athleteIdentifier,
                Name = name,
                AccessToken = accessToken,
                SignupDateTimeUtc = DateTime.UtcNow,
                LatestActivityId = latestActivity?.Id,
                Activities = activities
            };

            var existingAthlete = await _athleteRepository.GetAthlete(athleteIdentifier);

            if (existingAthlete == null)
            {
                await _athleteRepository.CreateOrUpdateAthlete(athlete);

                var retrievedAthlete = await GetAthlete(accessToken);

                await _slackService.PostSlackMessage($"Welcome, {retrievedAthlete.FirstName} {retrievedAthlete.LastName}!");
            }

            return athlete;
        }

        private async Task<FullAthlete> GetAthlete(string accessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://www.strava.com/api/v3/athlete");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var activitiesResponse = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<FullAthlete>(activitiesResponse);
            }

            return null;
        }

        public async Task<List<Activity>> GetRecentActivities(string accessToken)
        {
            var activities = new List<Activity>();

            var since = DateTime.UtcNow.AddDays(-DaysOfActivities).Date;

            var timestamp = (int) (since.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://www.strava.com/api/v3/athlete/activities?after={timestamp}");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var activitiesResponse = await response.Content.ReadAsStringAsync();

                activities = JsonConvert.DeserializeObject<List<Activity>>(activitiesResponse);
            }

            return activities;
        }
    }
}
