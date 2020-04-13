using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using ExerciseDaemon.Models.Google;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;

namespace ExerciseDaemon.ExternalServices
{
    public class GoogleMapsService
    {
        private const string CloudFrontBaseUrl = "https://d3p3wfpsct07a1.cloudfront.net/";

        private readonly GoogleSettings _googleSettings;
        private readonly S3Settings _s3Settings;
        private readonly ILogger<GoogleMapsService> _logger;
        private readonly HttpClient _client;

        public GoogleMapsService(GoogleSettings googleSettings, S3Settings s3Settings, ILogger<GoogleMapsService> logger)
        {
            _googleSettings = googleSettings;
            _s3Settings = s3Settings;
            _logger = logger;
            _client = new HttpClient();
        }

        public async Task<string> BuildMap(long activityId, string summaryPolyline)
        {
            _logger.LogInformation("1");

            var decodedSummaryPolyline = GooglePoints.Decode(summaryPolyline).ToList();

            _logger.LogInformation("2");

            var path = string.Join("|", decodedSummaryPolyline.Select(p => $"{p.Latitude},{p.Longitude}"));

            _logger.LogInformation("3");
            _logger.LogInformation(path);

            var formatString = @"https://maps.googleapis.com/maps/api/staticmap?maptype=roadmap&size=800x800&path={0}&key={1}&size=800x800";

            var url = string.Format(formatString, path, _googleSettings.ApiKey);

            _logger.LogInformation("4");
            _logger.LogInformation(url);
            await Task.Delay(5000);

            var response = await _client.GetAsync(url);

            _logger.LogInformation("5");
            _logger.LogInformation(response.StatusCode.ToString());
            await Task.Delay(5000);

            var result = await response.Content.ReadAsByteArrayAsync();

            _logger.LogInformation("6");
            await Task.Delay(5000);

            var s3Filename = $"{activityId}.png";

            using (var image = Image.Load(result))
            {
                image.Save(s3Filename);
            }

            _logger.LogInformation("7");
            await Task.Delay(5000);
            
            await UploadFileToS3(s3Filename);

            _logger.LogInformation("8");
            await Task.Delay(5000);

            return $"{CloudFrontBaseUrl}{s3Filename}";
        }

        public async Task UploadFileToS3(string s3Filename)
        {
            if (!string.IsNullOrWhiteSpace(_s3Settings.AccessKey) && !string.IsNullOrWhiteSpace(_s3Settings.SecretKey))
            {
                using (var client = new AmazonS3Client(_s3Settings.AccessKey, _s3Settings.SecretKey, RegionEndpoint.USEast1))
                {
                    await Upload(s3Filename, client);
                }
            }
            else
            {
                using (var client = new AmazonS3Client(RegionEndpoint.USEast1))
                {
                    await Upload(s3Filename, client);
                }
            }
        }

        private async Task Upload(string s3Filename, IAmazonS3 client)
        {
            using (var ms = new MemoryStream())
            using (var file = new FileStream(s3Filename, FileMode.Open, FileAccess.Read))
            {
                _logger.LogInformation($"File length: {file.Length}");

                var bytes = new byte[file.Length];

                file.Read(bytes, 0, (int) file.Length);

                ms.Write(bytes, 0, (int) file.Length);

                ms.Position = 0;

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = ms,
                    Key = s3Filename,
                    BucketName = "exercise-daemon",
                    CannedACL = S3CannedACL.PublicRead
                };

                var fileTransferUtility = new TransferUtility(client);
                await fileTransferUtility.UploadAsync(uploadRequest);
            }
        }
    }
}
