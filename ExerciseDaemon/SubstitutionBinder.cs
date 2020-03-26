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
            var stravaSettings = _configuration.GetSection("Strava").Get<StravaSettings>();

            if (stravaSettings.ClientId == default)
            {
                stravaSettings = new StravaSettings
                {
                    ClientId = int.Parse(Environment.GetEnvironmentVariable("ClientId")),
                    ClientSecret = Environment.GetEnvironmentVariable("ClientSecret")
                };
            }

            return _configuration.GetSection("Strava").Get<StravaSettings>();
        }

        public DynamoDbSettings BuildDynamoDbSettings(IServiceCollection services)
        {
            return _configuration.GetSection("DynamoDb").Get<DynamoDbSettings>();
        }

        public SlackSettings BuildSlackSettings(IServiceCollection services)
        {
            return _configuration.GetSection("Slack").Get<SlackSettings>();
        }
    }
}