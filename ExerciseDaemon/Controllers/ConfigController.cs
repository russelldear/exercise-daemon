using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExerciseDaemon.ExternalServices;
using ExerciseDaemon.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ExerciseDaemon.Controllers
{
    public class ConfigController : Controller
    {
        private readonly ISubstitutionBinder _substitutionBinder;
        private readonly SlackService _slackService;
        private readonly ILogger<ConfigController> _logger;

        public ConfigController(ISubstitutionBinder substitutionBinder, SlackService slackService, ILogger<ConfigController> logger)
        {
            _substitutionBinder = substitutionBinder;
            _slackService = slackService;
            _logger = logger;
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

            _logger.LogInformation("Config page visited.");

            return View(config);
        }
    }
}