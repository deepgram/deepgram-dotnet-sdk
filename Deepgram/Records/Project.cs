namespace Deepgram.Records;

public record Project
{
    [JsonPropertyName("project_id")]
    public string? ProjectId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

