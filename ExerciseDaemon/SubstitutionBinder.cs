using System;
using ExerciseDaemon.ExternalServices;
using ExerciseDaemon.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExerciseDaemon
{
    public interface ISubstitutionBinder
    {
        StravaSettings BuildStravaSettings(IServiceCollection services);
        
        DynamoDbSettings BuildDynamoDbSettings(IServiceCollection services);

        SlackSettings BuildSlackSettings(IServiceCollection services);
    }

    public class SubstitutionBinder : ISubstitutionBinder
    {
        private readonly IConfiguration _configuration;

        public SubstitutionBinder(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public StravaSettings BuildStravaSettings(IServiceCollection services)
        {
            StravaSettings stravaSettings = new StravaSettings();

            try
            {
                stravaSettings = _configuration.GetSection("Strava").Get<StravaSettings>();
            }
            catch { }

            if (stravaSettings.ClientId == default)
            {
                stravaSettings = new StravaSettings
                {
                    ClientId = int.Parse(Environment.GetEnvironmentVariable("StravaClientId")),
                    ClientSecret = Environment.GetEnvironmentVariable("StravaClientSecret")
                };
            }

            return stravaSettings;
        }

        public DynamoDbSettings BuildDynamoDbSettings(IServiceCollection services)
        {
            return _configuration.GetSection("DynamoDb").Get<DynamoDbSettings>();
        }

        public SlackSettings BuildSlackSettings(IServiceCollection services)
        {
            SlackSettings slackSettings = new SlackSettings();

            try
            {
                slackSettings = _configuration.GetSection("Slack").Get<SlackSettings>();
            }
            catch { }

            if (string.IsNullOrWhiteSpace(slackSettings.BaseUrl))
            {
                slackSettings = new SlackSettings
                {
                    BaseUrl = Environment.GetEnvironmentVariable("SlackUrl")
                };
            }

            return slackSettings;
        }
    }
}