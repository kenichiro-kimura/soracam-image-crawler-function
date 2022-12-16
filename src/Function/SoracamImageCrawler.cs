namespace soracamChecker
{
    public class SoracamImageCrawler
    {
        private readonly SoracomOptions soracomOptions;
        private readonly ApplicationOptions applicationOptions;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IBlobUploader blobUploader;

        public SoracamImageCrawler(
            IOptionsMonitor<SoracomOptions> soracomOptionsMonitor,
            IOptionsMonitor<ApplicationOptions> applicationOptionsMonitor,
            IHttpClientFactory httpClientFactory,
            IBlobUploader blobUploader
            )
        {
            this.soracomOptions = soracomOptionsMonitor?.CurrentValue ?? throw new ArgumentNullException(nameof(soracomOptions));
            this.applicationOptions = applicationOptionsMonitor?.CurrentValue ?? throw new ArgumentNullException(nameof(applicationOptions));
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.blobUploader = blobUploader ?? throw new ArgumentNullException(nameof(blobUploader));
        }

        [FunctionName("GetSoracamImage")]
        public async Task GetSoracamImage([TimerTrigger("0 */5 * * * *",RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            var auth = new AuthApi();

            var authRequest = new AuthRequest
            {
                Email = soracomOptions.Email,
                UserName = soracomOptions.UserName,
                AuthKeyId = soracomOptions.AuthKeyId,
                AuthKey = soracomOptions.AuthKey,
                Password = soracomOptions.Password,
                OperatorId = soracomOptions.OperatorId
            };

            var authResponse = auth.Auth(authRequest);

            var configuration = new Configuration();
            configuration.AddApiKey("X-Soracom-Token", authResponse.Token);
            configuration.AddApiKey("X-Soracom-API-Key", authResponse.ApiKey);

            var soracamApi = new SoraCamApi(configuration);

            DateTimeOffset now = DateTimeOffset.UtcNow;
            var deviceId = applicationOptions.SoraCamDeviceId;
            var targetTime = now.ToUnixTimeMilliseconds();
            var year = now.Year;
            var month = now.Month;
            var day = now.Month;
            var hour = now.Hour;
            var min = now.Minute;

            var soraCamImageExportSpecification = new SoraCamImageExportSpecification(time: targetTime);

            var soraCamExportImageResult = soracamApi.ExportSoraCamDeviceRecordedImage(deviceId, soraCamImageExportSpecification);

            SoraCamImageExportInfo exportInfo;
            var retryCount = 10;
            do
            {
                await Task.Delay(1000);
                exportInfo = soracamApi.GetSoraCamDeviceExportedImage(deviceId, soraCamExportImageResult.ExportId);
            } while (string.IsNullOrWhiteSpace(exportInfo.Url) && retryCount-- > 0);

            if (string.IsNullOrWhiteSpace(exportInfo.Url))
            {
                log.LogInformation($"fail get an exported image : export id is ${soraCamExportImageResult.ExportId}");
                throw new Exception();
            }
                
            var client = httpClientFactory.CreateClient("soracam"); 
            HttpResponseMessage res = await client.GetAsync(exportInfo.Url, HttpCompletionOption.ResponseContentRead);

            BlobClient blobClient = await blobUploader.GetBlobClientAsync($"{year}-{month}-{day}/{deviceId}/{year}-{month}-{day}-{hour}-{min}-{targetTime}.jpg", applicationOptions.BlobContainerName);

            using (var httpStream = await res.Content.ReadAsStreamAsync())
                await blobUploader.UploadAsync(blobClient,httpStream);

            var logout = new AuthApi(configuration);
            logout.Logout();
        }
    }
}
