using System.Threading.Tasks;
using ExerciseDaemon.ExternalServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace ExerciseDaemon.Controllers
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
            var slackUserId = Request.Cookies["SlackUserId"];

            if (string.IsNullOrWhiteSpace(slackUserId))
            {
                return RedirectToAction("Connect", "Slack");
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}