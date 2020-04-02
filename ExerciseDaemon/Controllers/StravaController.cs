using System;
using System.Threading.Tasks;
using ExerciseDaemon.ExternalServices;
using ExerciseDaemon.Models;
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
            try
            {
                var slackUserId = Request.Cookies["SlackUserId"];

                if (string.IsNullOrWhiteSpace(slackUserId))
                {
                    return RedirectToAction("Connect", "Slack");
                }

                return RedirectToAction("Index", "Home");

            }
            catch (Exception e)
            {
                var error = $"I'm sorry you had to see this. Could you send it to Russ please? {Environment.NewLine} {e.Message} - {e.StackTrace}";

                return View(new StravaViewModel {Error = error});
            }
        }

        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}