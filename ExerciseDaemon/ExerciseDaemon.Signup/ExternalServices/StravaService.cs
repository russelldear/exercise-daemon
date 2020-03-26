using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ExerciseDaemon.Signup.Models;
using ExerciseDaemon.Signup.Models.Strava;
using ExerciseDaemon.Signup.Repositories;
using Newtonsoft.Json;

namespace ExerciseDaemon.Signup.ExternalServices
{
    public class StravaService
    {
        private readonly AthleteRepository _athleteRepository;
        private readonly SlackService _slackService;
        private HttpClient _client;

        public StravaService(AthleteRepository athleteRepository, SlackService slackService)
        {
            _athleteRepository = athleteRepository;
            _slackService = slackService;

            _client = new HttpClient();
        }

        public async Task<AthleteViewModel> GetOrCreateAthlete(string accessToken, int athleteIdentifier)
        {
            var activities = await GetRecentActivities(accessToken);

            var latestActivity = activities.OrderByDescending(a => a.StartDateLocal).FirstOrDefault();

            var athlete = new AthleteViewModel
            {
                Id = athleteIdentifier,
                SignupDateTimeUtc = DateTime.UtcNow,
                LatestActivityDateTimeUtc = latestActivity?.StartDateLocal,
                Activities = activities
            };

            var existingAthlete = await _athleteRepository.GetAthlete(athleteIdentifier);

            if (existingAthlete == null)
            {
                await _athleteRepository.CreateAthlete(athlete);

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

        private async Task<List<Activity>> GetRecentActivities(string accessToken)
        {
            var activities = new List<Activity>();

            var yesterday = DateTime.UtcNow.AddDays(-30).Date;

            var timestamp = (int) (yesterday.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

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
