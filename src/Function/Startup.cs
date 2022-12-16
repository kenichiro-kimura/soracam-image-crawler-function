[assembly: FunctionsStartup(typeof(Startup))]
namespace Function
{
    class ConnectionString
    {
        public string StorageConnectionString { get; set; }
    }

    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<SoracomOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection(nameof(SoracomOptions)).Bind(settings);
                });
            builder.Services.AddOptions<ApplicationOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection(nameof(ApplicationOptions)).Bind(settings);
                });
            builder.Services.AddHttpClient("soracam", c => c.BaseAddress = new Uri("https://soracom-sora-cam-devices-api-export-file-prod.s3.amazonaws.com/"));
            builder.Services.AddScoped<IBlobUploader, BlobUploader>();
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();

            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: true, reloadOnChange: false)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();
        }
    }
}