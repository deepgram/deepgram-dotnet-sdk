namespace Deepgram.Records;
public record AsyncPrerecordedResponse
{
    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }
}

