using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Common;
using Deepgram.Interfaces;
using Deepgram.Models;
using Deepgram.Request;

namespace Deepgram.Clients
{
    internal class KeyClient : IKeyClient
    {
        private ApiRequest _apiRequest;

        public KeyClient(ApiRequest apiRequest)
        {
            _apiRequest = apiRequest;
        }

        /// <summary>
        /// Returns all Deepgram API keys associated with the project provided
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to retrieve keys from</param>
        /// <returns>List of Deepgram API keys</returns>
        public async Task<KeyList> ListKeysAsync(string projectId)
        {
            return await _apiRequest.DoRequestAsync<KeyList>(
                HttpMethod.Get,
                $"/v1/projects/{projectId}/keys");
        }

        /// <summary>
        /// Retrieves the Deepgram key associated with the provided keyId
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to retrieve key from</param>
        /// <param name="keyId">Unique identifier of the API key to retrieve</param>
        /// <returns>A Deepgram API key</returns>
        public async Task<Key> GetKeyAsync(string projectId, string keyId)
        {
            return await _apiRequest.DoRequestAsync<Key>(
                HttpMethod.Get,
                $"/v1/projects/{projectId}/keys/{keyId}");
        }

        /// <summary>
        /// Creates a new Deepgram API key
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to create the key under</param>
        /// <param name="comment">Comment to help identify the API key</param>
        /// <param name="scopes">Scopes associated with the key. Cannot be empty</param>
        /// <returns>A new Deepgram API key</returns>
        public async Task<ApiKey> CreateKeyAsync(string projectId, string comment, string[] scopes)
        {
            return await _apiRequest.DoRequestAsync<ApiKey>(
                HttpMethod.Post,
                $"/v1/projects/{projectId}/keys",
                null,
                new { comment, scopes }
            );
        }

        /// <summary>
        /// Deletes an API key
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to delete</param>
        /// <param name="keyId">Unique identifier of the API key to delete</param>
        public async Task<MessageResponse> DeleteKeyAsync(string projectId, string keyId)
        {
            return await _apiRequest.DoRequestAsync<MessageResponse>(
                HttpMethod.Delete,
                $"/v1/projects/{projectId}/keys/{keyId}");
        }
    }
}
