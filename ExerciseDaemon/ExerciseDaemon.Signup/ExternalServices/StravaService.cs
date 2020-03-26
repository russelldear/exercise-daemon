using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ExerciseDaemon.Signup.Models.Strava;
using ExerciseDaemon.Signup.Repositories;
using Newtonsoft.Json;

namespace ExerciseDaemon.Signup.ExternalServices
{
    public class StravaService
    {
        private readonly AthleteRepository _athleteRepository;

        public StravaService(AthleteRepository athleteRepository)
        {
            _athleteRepository = athleteRepository;
        }

        public async Task CreateAthlete(string accessToken, int athleteIdentifier)
        {
            var existingAthlete = await _athleteRepository.GetAthlete(athleteIdentifier);

            if (existingAthlete == null)
            {
                var activities = await GetRecentActivities(accessToken);

                var latestActivity = activities.OrderByDescending(a => a.StartDate).FirstOrDefault();

                var athlete = new Athlete
                {
                    Id = athleteIdentifier,
                    SignupDateTimeUtc = DateTime.UtcNow,
                    LatestActivityDateTimeUtc = latestActivity?.StartDate
                };

                await _athleteRepository.CreateAthlete(athlete);
            }
        }

        private async Task<List<Activity>> GetRecentActivities(string accessToken)
        {
            var activities = new List<Activity>();

            var yesterday = DateTime.UtcNow.AddDays(-30).Date;

            var timestamp = (int) (yesterday.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://www.strava.com/api/v3/athlete/activities?after={timestamp}");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var client = new HttpClient();

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var activitiesResponse = await response.Content.ReadAsStringAsync();

                activities = JsonConvert.DeserializeObject<List<Activity>>(activitiesResponse);
            }

            return activities;
        }
    }
}
