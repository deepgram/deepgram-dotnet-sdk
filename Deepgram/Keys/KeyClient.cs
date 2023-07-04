using System;
using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Common;
using Deepgram.Request;

namespace Deepgram.Keys
{
    public sealed class KeyClient : BaseClient, IKeyClient
    {
        public KeyClient(CleanCredentials credentials) : base(credentials) { }

        /// <summary>
        /// Returns all Deepgram API keys associated with the project provided
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to retrieve keys from</param>
        /// <returns>List of Deepgram API keys</returns>
        public async Task<KeyList> ListKeysAsync(string projectId)
        {
            return await ApiRequest.DoRequestAsync<KeyList>(
                HttpMethod.Get,
                $"projects/{projectId}/keys",
                Credentials
            );
        }

        /// <summary>
        /// Retrieves the Deepgram key associated with the provided keyId
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to retrieve key from</param>
        /// <param name="keyId">Unique identifier of the API key to retrieve</param>
        /// <returns>A Deepgram API key</returns>
        public async Task<Key> GetKeyAsync(string projectId, string keyId)
        {
            return await ApiRequest.DoRequestAsync<Key>(
                HttpMethod.Get,
                $"projects/{projectId}/keys/{keyId}",
                Credentials
            );
        }

        /// <summary>
        /// Creates a new Deepgram API key
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to create the key under</param>
        /// <param name="comment">Comment to help identify the API key</param>
        /// <param name="scopes">Scopes associated with the key. Cannot be empty</param>
        /// <returns>A new Deepgram API key</returns>
        public async Task<ApiKey> CreateKeyAsync(string projectId, string comment, string[] scopes, CreateKeyOptions createKeyOptions = null)
        {
            //endpoint expects  property names for CreateKeyOptions when set: tags, expiration_date,time_to_live_in_seconds
            // passing CreateKeyOptions directly causes the  endpoint to ignore the options passed in
            // endpoint expects CreateKeyOptions as root properties NOT as properties of CreateKeyOptions in the json body 
            string[] tags = null;
            Nullable<DateTime> expiration_date = null;
            Nullable<int> time_to_live_in_seconds = null;

            if (createKeyOptions != null)
            {
                if (createKeyOptions.ExpirationDate != null && createKeyOptions.TimeToLive != null)
                {
                    throw new ArgumentException(" Please provide expirationDate or timeToLive or neither. Providing both is not allowed.");
                }

                tags = createKeyOptions.Tags;
                expiration_date = createKeyOptions.ExpirationDate;
                time_to_live_in_seconds = createKeyOptions.TimeToLive;
            }
            return await ApiRequest.DoRequestAsync<ApiKey>(
                HttpMethod.Post,
                $"projects/{projectId}/keys",
                Credentials,
                new { comment, scopes, expiration_date, time_to_live_in_seconds, tags }
            );
        }

        /// <summary>
        /// Deletes an API key
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to delete</param>
        /// <param name="keyId">Unique identifier of the API key to delete</param>
        public async Task<MessageResponse> DeleteKeyAsync(string projectId, string keyId)
        {
            return await ApiRequest.DoRequestAsync<MessageResponse>(
                HttpMethod.Delete,
                $"projects/{projectId}/keys/{keyId}",
                Credentials);
        }
    }
}
