﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using ExerciseDaemon.Models.Strava;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static ExerciseDaemon.Constants.DocumentProperties;

namespace ExerciseDaemon.Repositories
{
    public class AthleteRepository
    {
        private const string AthletesTableName = "ExerciseDaemon-Athletes";

        private readonly Table _athletesTable;
        private readonly ILogger<AthleteRepository> _logger;

        public AthleteRepository(DynamoDbSettings dynamoDbSettings, ILogger<AthleteRepository> logger)
        {
            AmazonDynamoDBConfig clientConfig;

            if (string.IsNullOrWhiteSpace(dynamoDbSettings.ServiceUrl))
            {
                clientConfig = new AmazonDynamoDBConfig { RegionEndpoint = RegionEndpoint.GetBySystemName(dynamoDbSettings.RegionEndpoint) };
            }
            else
            {
                clientConfig = new AmazonDynamoDBConfig { ServiceURL = dynamoDbSettings.ServiceUrl };
            }

            var dynamoDbClient = new AmazonDynamoDBClient(clientConfig);

            _athletesTable = Table.LoadTable(dynamoDbClient, AthletesTableName);

            _logger = logger;
        }

        public async Task CreateOrUpdateAthlete(Athlete athlete)
        {
            var asJson = JsonConvert.SerializeObject(athlete);

            var item = Document.FromJson(asJson);

            await _athletesTable.PutItemAsync(item);

            _logger.LogInformation("Athlete created/updated.");
        }

        public async Task<Athlete> GetAthlete(int id)
        {
            var config = new GetItemOperationConfig
            {
                AttributesToGet = new List<string> { Id, Name, AccessToken, RefreshToken, ExpiresAt, SlackUserId, SignupDateTimeUtc, ReminderCount, LastReminderDateTimeUtc, LatestActivityId },
                ConsistentRead = true
            };

            var document = await _athletesTable.GetItemAsync(id, config);

            return document?.ToAthlete();
        }

        public async Task<List<Athlete>> GetAthletes()
        {
            var scanFilter = new ScanFilter();

            var search = _athletesTable.Scan(scanFilter);

            var documentList = new List<Document>();

            do
            {
                documentList.AddRange(await search.GetNextSetAsync());

            } while (!search.IsDone);

            return documentList.ToAthletes();
        }
    }
}
