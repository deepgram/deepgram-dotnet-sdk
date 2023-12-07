namespace Deepgram.Records;

public record GetProjectUsageRequestResponse
{
    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }

    [JsonPropertyName("created")]
    public string? Created { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("api_key_id")]
    public string? ApiKeyId { get; set; }

    [JsonPropertyName("response")]
    public Response? Response { get; set; }

    [JsonPropertyName("callback")]
    public Callback? Callback { get; set; }
}
