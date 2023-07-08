namespace Deepgram.Clients;
public class KeyClient : IKeyClient
{
    private ApiRequest _apiRequest;
    internal KeyClient(ApiRequest apiRequest)
    {
        _apiRequest = apiRequest;
    }

    /// <summary>
    /// Returns all Deepgram API keys associated with the project provided
    /// </summary>
    /// <param name="projectId">Unique identifier of the project to retrieve keys from</param>
    /// <returns>List of Deepgram API keys</returns>
    public async Task<KeyList> ListKeysAsync(string projectId) =>
        await _apiRequest.SendHttpRequestAsync<KeyList>(
                HttpMethod.Get,
                $"projects/{projectId}/keys");

    /// <summary>
    /// Retrieves the Deepgram key associated with the provided keyId
    /// </summary>
    /// <param name="projectId">Unique identifier of the project to retrieve key from</param>
    /// <param name="keyId">Unique identifier of the API key to retrieve</param>
    /// <returns>A Deepgram API key</returns>
    public async Task<Key> GetKeyAsync(string projectId, string keyId) =>
        await _apiRequest.SendHttpRequestAsync<Key>(
                HttpMethod.Get,
                $"projects/{projectId}/keys{keyId}");

    /// <summary>
    /// Creates a new Deepgram API key
    /// </summary>
    /// <param name="projectId">Unique identifier of the project to create the key under</param>
    /// <param name="comment">Comment to help identify the API key</param>
    /// <param name="scopes">Scopes associated with the key. Cannot be empty</param>
    /// <param name="createKeyOptions">options to configure key</param>
    /// <returns>A new Deepgram API key</returns>
    public async Task<ApiKey> CreateKeyAsync(string projectId, string comment, string[] scopes, CreateKeyOptions? createKeyOptions = null)
    {
        //endpoint expects  property names for CreateKeyOptions when set: tags, expiration_date,time_to_live_in_seconds
        // passing CreateKeyOptions directly causes the  endpoint to ignore the options passed in
        // endpoint expects CreateKeyOptions as root properties NOT as properties of CreateKeyOptions in the json body 
        string[]? tags = null;
        DateTime? expiration_date = null;
        int? time_to_live_in_seconds = null;

        if (createKeyOptions is not null)
        {
            if (createKeyOptions.ExpirationDate is not null && createKeyOptions.TimeToLive is not null)
            {
                throw new ArgumentException(" Please provide expirationDate or timeToLive or neither. Providing both is not allowed.");
            }

            tags = createKeyOptions.Tags;
            expiration_date = createKeyOptions.ExpirationDate;
            time_to_live_in_seconds = createKeyOptions.TimeToLive;
        }

        return await _apiRequest.SendHttpRequestAsync<ApiKey>(
                HttpMethod.Post,
                $"projects/{projectId}/keys",
                new { comment, scopes, expiration_date, time_to_live_in_seconds, tags });
    }

    /// <summary>
    /// Deletes an API key
    /// </summary>
    /// <param name="projectId">Unique identifier of the project to delete</param>
    /// <param name="keyId">Unique identifier of the API key to delete</param>
    public async Task<MessageResponse> DeleteKeyAsync(string projectId, string keyId) =>
        await _apiRequest.SendHttpRequestAsync<MessageResponse>(
                HttpMethod.Delete,
                $"projects/{projectId}/keys/{keyId}");
}
