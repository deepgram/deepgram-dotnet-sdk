namespace Deepgram.Models.Schemas;
public class CreateProjectKeySchema
{
    /// <summary>
    /// Comment to describe key
    /// </summary>
    [JsonPropertyName("comment")]
    public string Comment { get; set; }

    /// <summary>
    /// Scopes of the key
    /// </summary>
    [JsonPropertyName("scopes")]
    public string[] Scopes { get; set; }

    /// <summary>
    /// Tag names for key
    /// </summary>
    [JsonPropertyName("tags")]
    public string[]? Tags { get; set; }

}
