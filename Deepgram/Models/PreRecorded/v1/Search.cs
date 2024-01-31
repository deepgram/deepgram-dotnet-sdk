namespace Deepgram.Models.PreRecorded.v1;

public record Search
{
    /// <summary>
    /// Term for which Deepgram is searching.
    /// </summary>
    [JsonPropertyName("query")]
    public string? Query { get; set; }

    /// <summary>
    /// ReadonlyList of <see cref="Hit"/>
    /// </summary>
    [JsonPropertyName("hits")]
    public IReadOnlyList<Hit>? Hits { get; set; }
}

