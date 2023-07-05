namespace Deepgram.Models;

public class UpdateScopeOptions
{
    /// <summary>
    /// New scope
    /// </summary>
    [JsonPropertyName("scope")]
    public string Scope { get; set; }
}
