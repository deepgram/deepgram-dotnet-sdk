
using Deepgram.Request;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System;
using System.Net.Http;

namespace Deepgram
{
    public sealed class Configuration
    {
        const string LOGGER_CATEGORY = "Deepgram.Configuration";

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Configuration()
        {
        }

        private Configuration()
        {
            var logger = Logger.LogProvider.GetLogger(LOGGER_CATEGORY);
            var builder = new ConfigurationBuilder()
                .AddJsonFile("settings.json", true, true)
                .AddJsonFile("appsettings.json", true, true)
            ;

            Settings = builder.Build();

            if (string.IsNullOrWhiteSpace(Settings["appSettings:Deepgram.Api.Key"]))
            {
                logger.LogInformation("No authentication found via configuration. Remember to provide your own.");
            }
            else
            {
                logger.LogInformation("Authentication provided via configuration");
            }
        }

        public static Configuration Instance { get; } = new Configuration();

        public IConfiguration Settings { get; }

        public HttpMessageHandler ClientHandler { get; set; }

        private HttpClient _client;
        public HttpClient Client
        {
            get
            {
                var reqPerSec = Instance.Settings["appSettings:Deepgram.RequestsPerSecond"];
                if (string.IsNullOrEmpty(reqPerSec))
                    return _client ?? (_client = ClientHandler == null ? new HttpClient() : new HttpClient(ClientHandler));

                var delay = 1 / double.Parse(reqPerSec);
                var execTimeSpanSemaphore = new TimeSpanSemaphore(1, TimeSpan.FromSeconds(delay));
                // TODO: this messes up the unit test mock if throttle config is set
                var handler = ClientHandler != null ? new ThrottlingMessageHandler(execTimeSpanSemaphore, ClientHandler) : new ThrottlingMessageHandler(execTimeSpanSemaphore);
                return _client ?? (_client = new HttpClient(handler));
            }
        }
    }
}
