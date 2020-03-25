using System.Diagnostics;
using ExerciseDaemon.Signup.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExerciseDaemon.Signup.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
