using System;
using Newtonsoft.Json;

namespace ExerciseDaemon.Models.Strava
{
    public class TokenSet
    {
        public TokenSet()
        { }

        public TokenSet(string accessToken, string refreshToken, DateTime expiresAt)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresAt = expiresAt;
        }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("expires_at")]
        public long ExpiresAtUnixTime 
        {
            set => ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(value).UtcDateTime;
        }

        public DateTime ExpiresAt { get; set; }
    }
}
