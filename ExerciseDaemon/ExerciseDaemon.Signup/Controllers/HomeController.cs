using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using ExerciseDaemon.Signup.ExternalServices;
using ExerciseDaemon.Signup.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using static ExerciseDaemon.Signup.Constants.ClaimTypes;

namespace ExerciseDaemon.Signup.Controllers
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
                    var athleteIdentifier = claimsIdentity.FindFirst(AthleteIdentifier);

                    var athlete = await _stravaService.GetOrCreateAthlete(accessToken, int.Parse(athleteIdentifier.Value));

                    athlete.Name = $"{claimsIdentity.FindFirst(FirstName).Value} {claimsIdentity.FindFirst(LastName).Value}";

                    athlete.StravaJoinDate = DateTime.ParseExact(claimsIdentity.FindFirst(StravaJoinDate).Value, "MM/dd/yyyy hh:mm:ss", null);

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
