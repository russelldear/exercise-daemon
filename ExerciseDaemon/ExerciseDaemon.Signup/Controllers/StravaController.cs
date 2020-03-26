using System.Security.Claims;
using System.Threading.Tasks;
using ExerciseDaemon.Signup.ExternalServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace ExerciseDaemon.Signup.Controllers
{
    public class StravaController : Controller
    {
        private readonly StravaService _stravaService;

        public StravaController(StravaService stravaService)
        {
            _stravaService = stravaService;
        }

        public IActionResult Connect()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "Strava/Connected" }, "Strava");
        }

        public async Task<IActionResult> Connected()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken != null && User.Identity is ClaimsIdentity claimsIdentity)
            {
                var athleteIdentifier = claimsIdentity.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

                await _stravaService.CreateAthlete(accessToken, int.Parse(athleteIdentifier.Value));

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
}