namespace Deepgram.Records.PreRecorded;

public record SyncPrerecordedResponse
{
    [JsonPropertyName("metadata")]
    public Metadata Metadata { get; set; }

    [JsonPropertyName("results")]
    public Result Results { get; set; }

}
