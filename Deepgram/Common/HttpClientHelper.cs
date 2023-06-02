using System;
using System.Net.Http;
using Deepgram.Request;

namespace Deepgram.Common
{
    public static class HttpClientHelper
    {
        public static HttpClient GetHttpClient()
        {
            var appSettingHelper = new AppSettingsHelper();
            var reqPerSec = appSettingHelper.GetRequestsPerSecond();

            if (reqPerSec == 0)
                return new HttpClient(new HttpClientHandler() { AllowAutoRedirect = true });

            var delay = 1 / reqPerSec;
            var execTimeSpanSemaphore = new TimeSpanSemaphore(1, TimeSpan.FromSeconds(delay));
            ThrottlingMessageHandler handler;

            handler = new ThrottlingMessageHandler(execTimeSpanSemaphore, new HttpClientHandler() { AllowAutoRedirect = true });
            return new HttpClient(handler);
        }

        /// <summary>
        /// Sets the Timeout of the HTTPClient used to send HTTP requests
        /// </summary>
        /// <param name="timeout">Timespan to wait before the request times out.</param>
        //public static void SetHttpClientTimeout(TimeSpan timeout)
        //{
        //    Configuration.Instance.Client.Timeout = timeout;
        //}
    }
}
