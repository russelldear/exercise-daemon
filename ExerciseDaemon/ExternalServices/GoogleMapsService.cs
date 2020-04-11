using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using ExerciseDaemon.Models.Google;
using SixLabors.ImageSharp;

namespace ExerciseDaemon.ExternalServices
{
    public class GoogleMapsService
    {
        private const string LocalFilename = "strava.png";
        private const string CloudFrontBaseUrl = "https://d3p3wfpsct07a1.cloudfront.net/";

        private readonly GoogleSettings _googleSettings;
        private readonly HttpClient _client;

        public GoogleMapsService(GoogleSettings googleSettings)
        {
            _googleSettings = googleSettings;
            _client = new HttpClient();
        }

        public async Task<string> BuildMap(long activityId, string summaryPolyline)
        {
            var decodedSummaryPolyline = GooglePoints.Decode(summaryPolyline).ToList();

            var path = string.Join("|", decodedSummaryPolyline.Select(p => $"{p.Latitude},{p.Longitude}"));

            var formatString = @"https://maps.googleapis.com/maps/api/staticmap?maptype=roadmap&size=800x800&path={0}&key={1}&size=800x800";

            var url = string.Format(formatString, path, _googleSettings.ApiKey);

            var response = await _client.GetAsync(url);

            var result = await response.Content.ReadAsByteArrayAsync();

            using (var image = Image.Load(result))
            {
                image.Save(LocalFilename);
            }

            var s3Filename = $"{activityId}.png";

            await UploadFileToS3(LocalFilename, s3Filename);

            return $"{CloudFrontBaseUrl}{s3Filename}";
        }

        public async Task UploadFileToS3(string localFilename, string s3Filename)
        {
            using (var client = new AmazonS3Client(RegionEndpoint.USEast1))
            {
                using (var ms = new MemoryStream())
                using (var file = new FileStream(localFilename, FileMode.Open, FileAccess.Read))
                {
                    var bytes = new byte[file.Length];

                    file.Read(bytes, 0, (int)file.Length);

                    ms.Write(bytes, 0, (int)file.Length);

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
}
