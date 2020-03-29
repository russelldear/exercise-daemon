using System;
using System.Collections.Generic;
using static ExerciseDaemon.Constants.StatementSetKeys;

namespace ExerciseDaemon.Helpers
{
    public class StatementRandomiser
    {
        private static readonly Random Random = new Random();

        private readonly Dictionary<string, List<string>> _statementBank = new Dictionary<string, List<string>>();

        
        private readonly List<string> _welcomeTodayPrompts = new List<string>
        {
            "Hope that {0} today was good. Great work getting busy!",
            "I bet that {0} today shook out some cobwebs. Nice one!",
            "Your body and mind will thank you for that {0} today. :fistbump:"
        };

        private readonly List<string> _welcomeRecentPrompts = new List<string>
        {
            "Good to see that {0} on {1} got you moving. Time for another one soon?",
            "Great work on that {0} on {1}. When's the next one?"
        };

        private readonly List<string> _welcomeNeverPrompts = new List<string>
        {
            "It doesn't look like you've logged any Strava activities in the last week. Might be time for a wee bit of exercise?",
            "No Strava activities for over a week! Got time for one today?"
        };

        public StatementRandomiser()
        {
            _statementBank.Add(WelcomeTodayPrompts, _welcomeTodayPrompts);
            _statementBank.Add(WelcomeRecentPrompts, _welcomeRecentPrompts);
            _statementBank.Add(WelcomeNeverPrompts, _welcomeNeverPrompts);
        }

        public string Get(string key)
        {
            var statementSet = _statementBank[key];

            var nextIndex = Random.Next(statementSet.Count);

            return statementSet[nextIndex];
        }
    }
}
