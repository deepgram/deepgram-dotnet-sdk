namespace Deepgram.Records;

public record CreateProjectKeyResponse
{
    [JsonPropertyName("api_key_id")]
    public string? ApiKeyId { get; set; }

    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("scopes")]
    public IReadOnlyList<string>? Scopes { get; set; }

    [JsonPropertyName("tags")]
    public IReadOnlyList<string>? Tags { get; set; }

    [JsonPropertyName("created")]
    public string? Created { get; set; }
}

