using System.Net.Http;
using System.Threading.Tasks;
using Deepgram.Interfaces;
using Deepgram.Models;
using Deepgram.Request;

namespace Deepgram.Clients
{
    internal class KeyClient : BaseClient, IKeyClient
    {
        public KeyClient(Credentials credentials) : base(credentials) { }


        /// <summary>
        /// Returns all Deepgram API keys associated with the project provided
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to retrieve keys from</param>
        /// <returns>List of Deepgram API keys</returns>
        public async Task<KeyList> ListKeysAsync(string projectId)
        {
            var req = RequestMessageBuilder.CreateHttpRequestMessage(
            HttpMethod.Get,
             $"projects/{projectId}/keys",
            _credentials);

            return await _apiRequest.SendHttpRequestAsync<KeyList>(req);


        }

        /// <summary>
        /// Retrieves the Deepgram key associated with the provided keyId
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to retrieve key from</param>
        /// <param name="keyId">Unique identifier of the API key to retrieve</param>
        /// <returns>A Deepgram API key</returns>
        public async Task<Key> GetKeyAsync(string projectId, string keyId)
        {
            var req = RequestMessageBuilder.CreateHttpRequestMessage(
            HttpMethod.Get,
             $"projects/{projectId}/keys{keyId}",
            _credentials);

            return await _apiRequest.SendHttpRequestAsync<Key>(req);
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
            var req = RequestMessageBuilder.CreateHttpRequestMessage(
              HttpMethod.Post,
              $"projects/{projectId}/keys",
              _credentials,
              new { comment, scopes });

            return await _apiRequest.SendHttpRequestAsync<ApiKey>(req);
        }

        /// <summary>
        /// Deletes an API key
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to delete</param>
        /// <param name="keyId">Unique identifier of the API key to delete</param>
        public async Task<MessageResponse> DeleteKeyAsync(string projectId, string keyId)
        {
            var req = RequestMessageBuilder.CreateHttpRequestMessage(
               HttpMethod.Delete,
                $"projects/{projectId}/keys/{keyId}",
            _credentials);

            return await _apiRequest.SendHttpRequestAsync<MessageResponse>(req);
        }
    }
}
