namespace Deepgram.Models.Responses;
public class VoidResponse
{
    [JsonPropertyName("error")]
    public Exception Error { get; set; }
}
