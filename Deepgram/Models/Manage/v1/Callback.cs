namespace Deepgram.Models.Manage.v1;

public record Callback
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("attempts")]
    public int? Attempts { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("code")]
    public int? Code { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("completed")]
    public string? Completed { get; set; }
}
