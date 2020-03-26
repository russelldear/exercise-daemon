using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using ExerciseDaemon.Signup.Models.Strava;
using Newtonsoft.Json;

namespace ExerciseDaemon.Signup.Repositories
{
    public class AthleteRepository
    {
        private const string AthletesTableName = "ExerciseDaemon-Athletes";

        private readonly Table _athletesTable;

        public AthleteRepository(DynamoDbSettings dynamoDbSettings)
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
        }

        public async Task CreateAthlete(Athlete athlete)
        {
            var asJson = JsonConvert.SerializeObject(athlete);

            var item = Document.FromJson(asJson);

            await _athletesTable.PutItemAsync(item);
        }

        public async Task<Athlete> GetAthlete(int id)
        {
            var config = new GetItemOperationConfig
            {
                AttributesToGet = new List<string> { "Id", "SignupDateTimeUtc", "LatestActivityDateTimeUtc" },
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
