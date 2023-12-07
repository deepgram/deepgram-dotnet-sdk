namespace Deepgram.Records.Live;

public record Channel
{
    [JsonPropertyName("alternatives")]
    public IReadOnlyList<Alternative> Alternatives { get; set; }
}
