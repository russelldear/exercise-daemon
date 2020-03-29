namespace ExerciseDaemon
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

        public static class DocumentProperties
        {
            public const string Id = "Id";
            public const string Name = "Name";
            public const string AccessToken = "AccessToken";
            public const string RefreshToken = "RefreshToken";
            public const string ExpiresAt = "ExpiresAt";
            public const string SignupDateTimeUtc = "SignupDateTimeUtc";
            public const string ReminderCount = "ReminderCount";
            public const string LastReminderDateTimeUtc = "LastReminderDateTimeUtc";
            public const string LatestActivityId = "LatestActivityId";
        }

        public static class StatementSetKeys
        {
            public const string WelcomeTodayPrompts = "WelcomeTodayPrompts";
            public const string WelcomeRecentPrompts = "WelcomeRecentPrompts";
            public const string WelcomeNeverPrompts = "WelcomeNeverPrompts";
            public const string RecordNewActivity = "RecordNewActivity";
            public const string WeekReminder = "WeekReminder";
            public const string FortnightReminder = "FortnightReminder";
            public const string MonthReminder = "MonthReminder";
        }
    }
}