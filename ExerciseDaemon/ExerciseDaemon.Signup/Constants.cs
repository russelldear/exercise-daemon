namespace ExerciseDaemon.Signup
{
    public static class Constants
    {
        public static class ClaimTypes
        {
            public const string AthleteIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
            public const string FirstName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";
            public const string LastName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";
            public const string StravaJoinDate = "urn:strava:created-at";
        }
    }
}