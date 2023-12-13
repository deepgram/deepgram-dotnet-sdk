namespace Deepgram.Records.Live;

public record Search
{
    [JsonPropertyName("query")]
    public string Query { get; set; }

    /// <summary>
    /// <see cref="Hit"/>
    /// </summary>
    [JsonPropertyName("hits")]
    public IReadOnlyList<Hit> Hits { get; set; }
}
