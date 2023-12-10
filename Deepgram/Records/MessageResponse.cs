namespace Deepgram.Records;

public record MessageResponse
{
    /// <summary>
    /// A message denoting the success of the operation
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }
}
