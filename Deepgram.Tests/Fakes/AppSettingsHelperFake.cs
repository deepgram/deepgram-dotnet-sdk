using Deepgram.Common;
using Deepgram.Logger;
using Deepgram.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Deepgram.Tests.Fakes;
internal class AppSettingsHelperFake
{
    const string LOGGER_CATEGORY = "Deepgram.Configuration";

    internal ILogger CurrentLogger;
    internal IConfiguration Configuration;
    public AppSettingsHelperFake()
    {
        CurrentLogger = LogProvider.GetLogger(LOGGER_CATEGORY);
    }

    public AppSettings FetchAppSettings()
    {

        var section = Configuration.GetSection("appSettings");
        AppSettings settings = new AppSettings();

        if (section != null)
        {
            settings.ApiKey = section.GetSection(Constants.API_KEY_SECTION).Value ?? string.Empty;
            settings.ApiUrl = section.GetSection(Constants.API_URL_SECTION).Value ?? string.Empty;
            settings.RequireSSL = section.GetSection(Constants.API_REQUIRE_SSL).Value ?? null;

        }

        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            CurrentLogger.LogInformation("No authentication found via configuration. Remember to provide your own.");
        }
        else
        {
            CurrentLogger.LogInformation("Authentication provided via configuration");
        }

        return settings;
    }

    internal double GetRequestsPerSecond()
    {
        var section = Configuration.GetSection("appSettings");
        if (section.GetSection(Constants.REQUESTS_PER_SECOND_SECTION).Value != null)
            return Convert.ToDouble(section.GetSection(Constants.REQUESTS_PER_SECOND_SECTION).Value);

        return default(double);
    }

}
