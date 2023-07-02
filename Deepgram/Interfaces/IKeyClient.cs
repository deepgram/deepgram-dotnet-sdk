using System.Threading.Tasks;
using Deepgram.Models;

namespace Deepgram.Interfaces
{
    public interface IKeyClient : IBaseClient
    {
        /// <summary>
        /// Returns all Deepgram API keys associated with the project provided
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to retrieve keys from</param>
        /// <returns>List of Deepgram API keys</returns>
        Task<KeyList> ListKeysAsync(string projectId);

        /// <summary>
        /// Retrieves the Deepgram key associated with the provided keyId
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to retrieve key from</param>
        /// <param name="keyId">Unique identifier of the API key to retrieve</param>
        /// <returns>A Deepgram API key</returns>
        Task<Key> GetKeyAsync(string projectId, string keyId);

        /// <summary>
        /// Creates a new Deepgram API key
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to create the key under</param>
        /// <param name="comment">Comment to help identify the API key</param>
        /// <param name="scopes">Scopes associated with the key. Cannot be empty</param>
        /// <returns>A new Deepgram API key</returns>
        Task<ApiKey> CreateKeyAsync(string projectId, string comment, string[] scopes, CreateKeyOptions createKeyOptions = null);

        /// <summary>
        /// Deletes an API key
        /// </summary>
        /// <param name="projectId">Unique identifier of the project to delete</param>
        /// <param name="keyId">Unique identifier of the API key to delete</param>
        Task<MessageResponse> DeleteKeyAsync(string projectId, string keyId);
    }
}
