using Microsoft.AspNetCore.Mvc;

namespace ExerciseDaemon.Controllers
{
    public class PingController : Controller
    {
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
