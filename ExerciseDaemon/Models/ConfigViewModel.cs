using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExerciseDaemon.Models
{
    public class ConfigViewModel
    {
        public string StravaClientId { get; set; }

        public string DynamoDbRegion { get; set; }

        public string DynamoDbUrl { get; set; }

        public string SlackUrl { get; set; }
    }
}
