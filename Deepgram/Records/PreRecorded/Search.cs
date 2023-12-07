namespace Deepgram.Records.PreRecorded;

public class Search
{
    [JsonPropertyName("query")]
    public string Query { get; set; }

    [JsonPropertyName("hits")]
    public IReadOnlyList<Hit> Hits { get; set; }
}

