using System;
using Deepgram.Common;
using Deepgram.Request;

namespace Deepgram.Utilities
{

    public class CredentialsUtil
    {
        /// <summary>
        /// Method for cleaning the passed in Credentials
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        internal static CleanCredentials Clean(Credentials credentials = null)
        {
            credentials = credentials ?? new Credentials();

            //Set values and clean them up 
            return new CleanCredentials(
                CheckApiKey(credentials.ApiKey),
                CleanApiUrl(credentials.ApiUrl),
                CleanRequireSSL(credentials.RequireSSL));
        }

        /// <summary>
        /// Checks the required ApiKey is present 
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
        internal static string TrimApiUrl(string apiUrl)
        {
            if (apiUrl.Contains("://"))
            {
                return apiUrl.Substring(apiUrl.IndexOf("://") + 3);
            }
            else
            {
                return apiUrl;
            }
        }

        /// <summary>
        /// converts value to boolean if passed in or sets default
        /// </summary>
        /// <param name="requireSSL"></param>
        /// <returns></returns>
        internal static bool CleanRequireSSL(bool? requireSSL = null) =>
            !requireSSL.HasValue || Convert.ToBoolean(requireSSL);

    }
}
