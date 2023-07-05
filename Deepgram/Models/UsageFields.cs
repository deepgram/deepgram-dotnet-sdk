namespace Deepgram.Models;

public class UsageFields
{
    /// <summary>
    /// Array of included tags.
    /// </summary>
    [JsonPropertyName("tags")]
    public string[] Tags { get; set; }

    /// <summary>
    /// Array of included models.
    /// </summary>
    [JsonPropertyName("models")]
    public string[] Models { get; set; }

    /// <summary>
    /// Array of included processing methods.
    /// </summary>
    [JsonPropertyName("processing_methods")]
    public RequestMethod[] ProcessingMethods { get; set; }

    /// <summary>
    /// Array of included languages.
    /// </summary>
    [JsonPropertyName("languages")]
    public string[] Languages { get; set; }

    /// <summary>
    /// Array of included features.
    /// </summary>
    [JsonPropertyName("features")]
    public string[] Features { get; set; }
}
