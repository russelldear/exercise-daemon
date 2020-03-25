using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ExerciseDaemon.Signup.Controllers
{
    public class StravaController : Controller
    {
        public IActionResult Connect()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "Strava/Connected" }, "Strava");
        }

        public async Task<IActionResult> Connected()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken != null && User.Identity is ClaimsIdentity claimsIdentity)
            {
                var athleteIdentifier = claimsIdentity.FindFirst("nameidentifier");

                var yesterday = DateTime.UtcNow.AddDays(-1).Date;

                var timestamp = (int)(yesterday.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://www.strava.com/api/v3/athlete/activities?after={timestamp}");

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var client = new HttpClient();

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var activitiesResponse = await response.Content.ReadAsStringAsync();

                    var activities = JsonConvert.DeserializeObject<List<Activity>>(activitiesResponse);
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }

    public class Activity
    {
        public long Id { get; set; }

        public Athlete Athlete { get; set; }

        public string Name { get; set; }
        
        public string Type { get; set; }

        [JsonProperty("private")]
        public bool IsPrivate { get; set; }

        public float Distance { get; set; }
        public DateTime StartDate { get; set; }

        public DateTime StartDateLocal { get; set; }

        public string Timezone { get; set; }

        public int UtcOffset { get; set; }

        public int SufferScore { get; set; }
    }

    public class Athlete
    {
        public int Id { get; set; }
    }

}