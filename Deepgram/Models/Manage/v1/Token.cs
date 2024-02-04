namespace Deepgram.Models.Manage.v1;

public record Token
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("in")]
    public int? In { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("out")]
    public int? Out { get; set; }
}
