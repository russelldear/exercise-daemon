using System;
using ExerciseDaemon.ExternalServices;
using ExerciseDaemon.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExerciseDaemon
{
    public interface ISubstitutionBinder
    {
        StravaSettings BuildStravaSettings();
        
        DynamoDbSettings BuildDynamoDbSettings();

        SlackSettings BuildSlackSettings();

        GoogleSettings BuildGoogleSettings();

        S3Settings BuildS3Settings();
    }

    public class SubstitutionBinder : ISubstitutionBinder
    {
        private readonly IConfiguration _configuration;

        public SubstitutionBinder(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public StravaSettings BuildStravaSettings()
        {
            var stravaSettings = new StravaSettings();

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

        public DynamoDbSettings BuildDynamoDbSettings()
        {
            return _configuration.GetSection("DynamoDb").Get<DynamoDbSettings>();
        }

        public SlackSettings BuildSlackSettings()
        {
            var slackSettings = new SlackSettings();

            try
            {
                slackSettings = _configuration.GetSection("Slack").Get<SlackSettings>();
            }
            catch { }

            if (string.IsNullOrWhiteSpace(slackSettings.SlackWebhookUrl))
            {
                slackSettings = new SlackSettings
                {
                    SlackWebhookUrl = Environment.GetEnvironmentVariable("SlackUrl"),
                    SlackWorkspaceUrl = Environment.GetEnvironmentVariable("SlackWorkspaceUrl"),
                    SlackChannelId = Environment.GetEnvironmentVariable("SlackChannelId"),
                    SlackBotUserAccessToken = Environment.GetEnvironmentVariable("SlackBotUserAccessToken"),
                    SlackClientId = Environment.GetEnvironmentVariable("SlackClientId"),
                    SlackClientSecret = Environment.GetEnvironmentVariable("SlackClientSecret"),
                    SlackRedirectUri = Environment.GetEnvironmentVariable("SlackRedirectUri")
                };
            }

            return slackSettings;
        }

        public GoogleSettings BuildGoogleSettings()
        {
            var googleSettings = new GoogleSettings();

            try
            {
                googleSettings = _configuration.GetSection("Google").Get<GoogleSettings>();
            }
            catch { }

            if (string.IsNullOrWhiteSpace(googleSettings.ApiKey))
            {
                googleSettings = new GoogleSettings
                {
                    ApiKey = Environment.GetEnvironmentVariable("GoogleApiKey")
                };
            }

            return googleSettings;
        }

        public S3Settings BuildS3Settings()
        {
            var s3Settings = new S3Settings();

            try
            {
                s3Settings = _configuration.GetSection("S3").Get<S3Settings>();
            }
            catch { }

            return s3Settings;
        }
    }
}