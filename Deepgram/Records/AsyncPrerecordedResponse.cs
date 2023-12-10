namespace Deepgram.Records;
public record AsyncPrerecordedResponse
{
    /// <summary>
    /// Id of transcription request returned when
    /// a callback has been supplied in request
    /// </summary>
    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }
}

