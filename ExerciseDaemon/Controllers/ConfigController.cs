using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExerciseDaemon.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExerciseDaemon.Controllers
{
    public class ConfigController : Controller
    {
        private readonly ISubstitutionBinder _substitutionBinder;

        public ConfigController(ISubstitutionBinder substitutionBinder)
        {
            _substitutionBinder = substitutionBinder;
        }

        public IActionResult Index()
        {
            var config = new ConfigViewModel();

            try
            {
                var stravaSettings = _substitutionBinder.BuildStravaSettings();

                config.StravaClientId = stravaSettings.ClientId.ToString();
            }
            catch (Exception e)
            {
                config.StravaClientId = e.Message + e.StackTrace;
            }

            try
            {
                var dynamoDbSettings = _substitutionBinder.BuildDynamoDbSettings();

                config.DynamoDbRegion = dynamoDbSettings.RegionEndpoint;
                config.DynamoDbUrl = dynamoDbSettings.ServiceUrl;
            }
            catch (Exception e)
            {
                config.DynamoDbRegion = e.Message + e.StackTrace;
            }

            try
            {
                var slackSettings = _substitutionBinder.BuildSlackSettings();

                config.SlackUrl = slackSettings.BaseUrl;
            }
            catch (Exception e)
            {
                config.SlackUrl = e.Message + e.StackTrace;
            }
            
            return View(config);
        }
    }
}