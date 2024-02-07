namespace Deepgram.Models.Analyze.v1;

public record SyncResponse
{
    /// <summary>
    /// Metadata of response <see cref="Metadata"/>
    /// </summary>
    [JsonPropertyName("metadata")]
    public Metadata? Metadata { get; set; }

    /// <summary>
    /// Results of Response <see cref="v1.Results"/>
    /// </summary>
    [JsonPropertyName("results")]
    public Results? Results { get; set; }
}
