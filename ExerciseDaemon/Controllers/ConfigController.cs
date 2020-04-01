using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExerciseDaemon.ExternalServices;
using ExerciseDaemon.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ExerciseDaemon.Controllers
{
    public class ConfigController : Controller
    {
        private readonly ISubstitutionBinder _substitutionBinder;
        private readonly SlackService _slackService;

        public ConfigController(ISubstitutionBinder substitutionBinder, SlackService slackService)
        {
            _substitutionBinder = substitutionBinder;
            _slackService = slackService;
        }

        public async Task<IActionResult> Index([FromQuery] string substitutionBindingMethod, [FromQuery] string slackId)
        {
            var config = new ConfigViewModel
            {
                ConfigValues = new List<string>()
            };

            if (!string.IsNullOrWhiteSpace(slackId))
            {
                var response = await _slackService.AddUserToChannel(slackId);

                config.ConfigValues.Add($"Response Content: {await response.Content.ReadAsStringAsync()}");
            }

            if (!string.IsNullOrWhiteSpace(substitutionBindingMethod))
            {
                var substitutionBinder = Type.GetType("ExerciseDaemon.SubstitutionBinder");
                var method = substitutionBinder.GetMethod(substitutionBindingMethod);
                var result = method.Invoke(_substitutionBinder, new object[0]);

                config.ConfigValues.Add(JsonConvert.SerializeObject(result));
            }

            return View(config);
        }
    }
}