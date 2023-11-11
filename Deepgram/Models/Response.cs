namespace Deepgram.Models;

public class Response
{
    [JsonPropertyName("details")]
    public Details? Details { get; set; }

    [JsonPropertyName("code")]
    public int? Code { get; set; }

    [JsonPropertyName("completed")]
    public string? Completed { get; set; }
}
