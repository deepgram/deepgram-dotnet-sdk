namespace Deepgram.Records;

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

    [JsonPropertyName("scopes")]
    public IReadOnlyList<string>? Scopes { get; set; }

    [JsonPropertyName("tags")]
    public IReadOnlyList<string>? Tags { get; set; }

    [JsonPropertyName("created")]
    public string? Created { get; set; }
}
