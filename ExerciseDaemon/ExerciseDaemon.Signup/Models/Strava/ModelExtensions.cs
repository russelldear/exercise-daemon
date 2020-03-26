using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DocumentModel;

namespace ExerciseDaemon.Signup.Models.Strava
{
    public static class ModelExtensions
    {
        public static Athlete ToAthlete(this Document document)
        {
            return new Athlete
            {
                Id = int.Parse(document["Id"]),
                SignupDateTimeUtc = DateTime.Parse(document["SignupDateTimeUtc"]),
                LatestActivityDateTimeUtc = DateTime.Parse(document["LatestActivityDateTimeUtc"])
            };
        }

        public static List<Athlete> ToAthletes(this List<Document> documents)
        {
            var result = new List<Athlete>();

            foreach (var document in documents)
            {
                result.Add(document.ToAthlete());
            }

            return result;
        }
    }
}