using System;

namespace Deepgram.Common
{
    public static class CleanCredentials
    {
        public static string CheckApiKey(string apiKey = null)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentException("Deepgram API Key must be provided in constructor");

            return apiKey;
        }

        public static string CleanApiUrl(string apiUrl = null) =>
            string.IsNullOrEmpty(apiUrl) ? "api.deepgram.com" : TrimApiUrl(apiUrl);

        internal static string TrimApiUrl(string apiUrl) =>
            apiUrl.Contains("://") ? apiUrl.Substring(apiUrl.IndexOf("://") + 3) : apiUrl;

        public static bool CleanRequireSSL(Nullable<bool> requireSSL = null) =>
            !requireSSL.HasValue ? true : Convert.ToBoolean(requireSSL);

    }
}
