namespace Deepgram.Models;

public class Search
{
    /// <summary>
    /// Term for which Deepgram is searching.
    /// </summary>
    [JsonPropertyName("query")]
    public string Query { get; set; }

    /// <summary>
    /// Array of Hit objects
    /// </summary>
    [JsonPropertyName("hits")]
    public Hit[] Hits { get; set; }
}
