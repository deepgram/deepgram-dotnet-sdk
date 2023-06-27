using System;
using Deepgram.Common;
using Deepgram.Models;

namespace Deepgram.Utilities
{
    public class CredentialsUtil
    {
        /// <summary>
        /// Method for cleaning the passed in Credentials
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public static Credentials Clean(Credentials credentials = null)
        {
            //if no credentials are passed in the constructor create a empty credentials
            if (credentials == null)
                credentials = new Credentials();

            //Set values and clean them up 
            return new Credentials(
                CheckApiKey(credentials.ApiKey),
                CleanApiUrl(credentials.ApiUrl),
                CleanRequireSSL(credentials.RequireSSL));

        }

        /// <summary>
        /// Checks the required apikey is present 
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        internal static string CheckApiKey(string apiKey = null)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentException("Deepgram API Key must be provided in constructor");

            return apiKey;
        }

        /// <summary>
        /// sets the cleaned ApiUri if passed in or sets default
        /// </summary>
        /// <param name="apiUrl"></param>
        /// <returns></returns>
        internal static string CleanApiUrl(string apiUrl = null) =>
            string.IsNullOrEmpty(apiUrl) ? Constants.DEFAULT_URI : TrimApiUrl(apiUrl);

        /// <summary>
        /// removes http:// or https:// from the uri
        /// </summary>
        /// <param name="apiUrl"></param>
        /// <returns></returns>
        internal static string TrimApiUrl(string apiUrl) =>
            apiUrl.Contains("://") ? apiUrl.Substring(apiUrl.IndexOf("://") + 3) : apiUrl;

        /// <summary>
        /// covnerts value to boolean if passed in or sets default
        /// </summary>
        /// <param name="requireSSL"></param>
        /// <returns></returns>
        internal static bool CleanRequireSSL(bool? requireSSL = null) =>
            !requireSSL.HasValue ? true : Convert.ToBoolean(requireSSL);

    }
}
