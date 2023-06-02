using System;
using System.IO;
using System.Reflection;
using Deepgram.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Deepgram.Common
{
    internal class AppSettingsHelper
    {

        const string LOGGER_CATEGORY = "Deepgram.Configuration";

        internal ILogger CurrentLogger;
        internal IConfiguration Configuration;
        public AppSettingsHelper()
        {
            CurrentLogger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
            Configuration = new ConfigurationBuilder()
                .SetBasePath(GetBasePath())
                .AddJsonFile("settings.json", true, true)
                .AddJsonFile("appsettings.json", true, true)
                .Build();
        }

        internal double GetRequestsPerSecond()
        {
            var section = Configuration.GetSection("appSettings");
            if (section.GetSection(Constants.REQUESTS_PER_SECOND_SECTION).Value != null)
                return Convert.ToDouble(section.GetSection(Constants.REQUESTS_PER_SECOND_SECTION).Value);

            return default(double);
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
        private string GetBasePath()
        {
            string basePath = Path.GetDirectoryName(typeof(AppSettingsHelper).GetTypeInfo().Assembly.Location);

            //Step up  directory folders till Json files found
            for (int i = 0; i < 5; i++)
            {
                if (File.Exists(Path.Combine(basePath, "appsettings.json")) || File.Exists(Path.Combine(basePath, "settings.json")))
                    return Path.GetFullPath(basePath);
                basePath += "..\\";
            }

            return Path.GetFullPath(".");
        }


    }
}
