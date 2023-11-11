namespace Deepgram.Models.Responses;

public class SyncPrerecordedResponse
{
    [JsonPropertyName("metadata")]
    public Metadata? Metadata { get; set; }

    [JsonPropertyName("results")]
    public Result? Results { get; set; }

}
