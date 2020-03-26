using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ExerciseDaemon.ExternalServices;
using ExerciseDaemon.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using static ExerciseDaemon.Constants.ClaimTypes;

namespace ExerciseDaemon.Controllers
{
    public class HomeController : Controller
    {
        private readonly StravaService _stravaService;

        public HomeController(StravaService stravaService)
        {
            _stravaService = stravaService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var accessToken = await HttpContext.GetTokenAsync("access_token");

                if (accessToken != null && User.Identity is ClaimsIdentity claimsIdentity)
                {
                    var athleteIdentifier = int.Parse(claimsIdentity.FindFirst(AthleteIdentifier).Value);

                    var athleteName = $"{claimsIdentity.FindFirst(FirstName).Value} {claimsIdentity.FindFirst(LastName).Value}";

                    var athlete = await _stravaService.GetOrCreateAthlete(accessToken, athleteIdentifier, athleteName);

                    athlete.StravaJoinDate = DateTime.Parse(claimsIdentity.FindFirst(StravaJoinDate).Value);

                    return View(athlete);
                }
            }

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
