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

        public IActionResult Connected()
        {
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}