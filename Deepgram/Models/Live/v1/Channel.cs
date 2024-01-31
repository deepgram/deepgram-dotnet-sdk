namespace Deepgram.Models.Live.v1;

public record Channel
{
    /// <summary>
    /// ReadOnlyList of <see cref="Alternative"/> objects.
    /// </summary>
    [JsonPropertyName("alternatives")]
    public IReadOnlyList<Alternative>? Alternatives { get; set; }

    /// <summary>
    /// <see cref="Search"/>
    /// </summary>
    [JsonPropertyName("search")]
    public IReadOnlyList<Search>? Searches { get; set; }
}
