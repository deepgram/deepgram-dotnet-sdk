namespace Deepgram.Models;

public class Search
{
    [JsonPropertyName("query")]
    public string? Query { get; set; }

    [JsonPropertyName("hits")]
    public Hit[]? Hits { get; set; }
}

