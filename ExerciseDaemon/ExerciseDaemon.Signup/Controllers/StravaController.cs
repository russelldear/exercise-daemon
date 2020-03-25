using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

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

                var request = new HttpRequestMessage(HttpMethod.Get, "");

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);



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