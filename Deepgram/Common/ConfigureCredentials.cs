using System;
using Deepgram.Models;

namespace Deepgram.Common
{
    public static class ConfigureCredentials
    {


        public static string ConfigureApiKey(AppSettings appSettings, string apiKey = null)
        {
            if (string.IsNullOrEmpty(apiKey) && string.IsNullOrEmpty(appSettings.ApiKey))
                throw new ArgumentException("Deepgram API Key must be provided in constructor or via settings");

            if (string.IsNullOrEmpty(apiKey))
                return appSettings.ApiKey;

            return apiKey;
        }



        public static string ConfigureApiUrl(AppSettings appSettings, string apiUrl = null)
        {
            if (string.IsNullOrEmpty(apiUrl))
            {
                return string.IsNullOrEmpty(appSettings.ApiUrl) ? "api.deepgram.com" : TrimApiUrl(appSettings.ApiUrl);
            }
            return TrimApiUrl(apiUrl);
        }

        internal static string TrimApiUrl(string apiUrl)
        {
            if (apiUrl.Contains("://"))
            {
                return apiUrl.Substring(apiUrl.IndexOf("://") + 3);
            }

            return apiUrl;
        }

        public static bool ConfigureRequireSSL(AppSettings appSettings, Nullable<bool> requireSSL = null)
        {

            if (!requireSSL.HasValue)
            {
                if (string.IsNullOrEmpty(appSettings.RequireSSL))
                {
                    return true;
                }
                else
                {
                    return Convert.ToBoolean(appSettings.RequireSSL);
                }

            }


            return Convert.ToBoolean(requireSSL);
        }


    }
}
