﻿using System;
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
            "Your body and mind will thank you for that {0} today. :fistbump:",
            "Looks like you smashed out a {0} today, you legend. Keep it up!",
            "I can see you're gonna be one to watch, what with that {0} today. I'm excited."
        };

        private readonly List<string> _welcomeRecentPrompts = new List<string>
        {
            "Good to see that {0} on {1} got you moving. Time for another one soon?",
            "Great work on that {0} on {1}. When's the next one?",
            "Good {0} on {1}? Hope so. Looking forward to seeing the next one.",
            "So {1} brought a {0}, I see - what will today bring? And tomorrow?",
            "{1} : {0}. Today : ???. Tomorrow : The World"
        };

        private readonly List<string> _welcomeNeverPrompts = new List<string>
        {
            "It doesn't look like you've logged any Strava activities in the last week. Might be time for a wee bit of exercise?",
            "No Strava activities for over a week! Got time for one today?",
            "Mate, I'm pretty keen to see an activity or two soon - doesn't look like you've been active in the last week.",
            "You, me, and some exercise today - whaddya say?",
            "1. Do some exercise. 2. Log it in Strava. 3. ???. 4. Profit!"
        };
        
        private readonly List<string> _recordNewActivity = new List<string>
        {
            "{0} just completed a {1}. Great work!",
            "Yusss! Spotted you smashing out that {1}, {0}. Sweetbix!",
            "One more {1} for the record books, {0}. Nice one!",
            "A wild {1} appears. {0} uses Strava attack. It's super effective!",
            "Oof, don't think I could have nailed that {1} as well as you did, {0}. Bloody awesome.",
            "+100 health points for that {1} just now, {0}.",
            "Super {1} action there, {0}. ",
            "Don't you just love it when a {1} plan comes together, {0}? Fine effort.",
            "Fantastic work {0} - that {1} just now is just the ticket.",
            "{0}, my friend, you are setting all the right examples with {1}s like that. Keep it up!",
            "<nods in silent appreciation at {0}'s {1}>",
            "...and coming in hot with a spiffy wee {1}, it's {0}! Woo!",
            "YES! {0}! YES! {1}! YES YES YES!",
            "Your {1} has just made my day, {0}. Muchas gracias.",
            "{0}, if I wasn't just a bunch of electrical charges stored in capacitors somewhere in Virginia, I would pat you on the back for that {1}.",
            "Snaps for the {1} star, {0}!",
            "I'm not saying that was a _perfect_ {1}, {0}, but I haven't seen a better one - and I've been doing this a looong time, know what I mean?",
            "Damn, {0}, back at it again with those slammin' {1}s.",
            "I liked you before, {0}, but after that {1} just now - well, let's just say you're #1 in my book.",
            "{1} long and prosper, Captain {0} :spock-hand:",
            "{0}. {1}. Winning.",
            "Another {1}? The (free non-alcoholic) drinks are on me, {0}.",
            "{0}'s {1} : legen.......wait for it...........dary.",
            "Are you icecream, {0}? 'Cause that {1} was tip-top.",
            "Ka rawe to mahi, e hoa!"
        };

        private readonly List<string> _weekReminder = new List<string>
        {
            "Hey, {0}, it's been a week since you were last active. Feel like getting up and about again?",
            "OK {0} - time to get out from behind that desk and get moving. Don't leave it another week without exercise!",
            "Seven days, {0}, and seven lost opportunities for exercise. Come on! Don't let me down!",
            "Your body and brain will love you if you get up and about {0}. It's been a week - let's go!",
            "{0}, {0}, {0}. Let's gooooooooooooooooooooooooooooooooooooooo!"
        };

        private readonly List<string> _fortnightReminder = new List<string>
        {
            "Hey, {0}, it's been a fortnight since you were last active. Feel like getting up and about again?",
            "Time's ticking away, {0}. I'm worried about you. Go do some exercise, yeah? For me, if for no other reason?",
            "It has been while, {0}, but I know you haven't forgotten me. Go get out and about, and all is forgiven.",
            "Health, wealth and happiness can all be yours, {0}, if you'd just log some exercise. Please?",
            "Couple of weeks now, {0}. It can't have been raining for that long. Li'l exercise on the menu, maybe?"
        };

        private readonly List<string> _monthReminder = new List<string>
        {
            "OK, {0}, it's been a month since you were last active. Imma leave you alone after this, but just know that I care for you, and I want you to be healthy and well. Look after yourself, mmmkay?"
        };

        public StatementRandomiser()
        {
            _statementBank.Add(WelcomeTodayPrompts, _welcomeTodayPrompts);
            _statementBank.Add(WelcomeRecentPrompts, _welcomeRecentPrompts);
            _statementBank.Add(WelcomeNeverPrompts, _welcomeNeverPrompts);
            _statementBank.Add(RecordNewActivity, _recordNewActivity);
            _statementBank.Add(WeekReminder, _weekReminder);
            _statementBank.Add(FortnightReminder, _fortnightReminder);
            _statementBank.Add(MonthReminder, _monthReminder);
        }

        public string Get(string key)
        {
            var statementSet = _statementBank[key];

            var nextIndex = Random.Next(statementSet.Count);

            return statementSet[nextIndex];
        }
    }
}
