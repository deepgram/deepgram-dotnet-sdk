namespace Deepgram.Models.Manage.v1;

public record Key
{
    /// <summary>
    /// Unique identifier of the Deepgram API key
    /// </summary>
    [JsonPropertyName("api_key_id")]
    public string? ApiKeyId { get; set; }

    /// <summary>
    /// Comment for the Deepgram API key
    /// </summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("scopes")]
    public IReadOnlyList<string>? Scopes { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("created")]
    public DateTime? Created { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("tags")]
    public IReadOnlyList<string>? Tags { get; set; }
}

