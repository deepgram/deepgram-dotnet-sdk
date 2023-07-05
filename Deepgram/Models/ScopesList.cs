namespace Deepgram.Models;

public class ScopesList
{
    /// <summary>
    /// Lists the specified project scopes assigned to the specified member
    /// </summary>
    [JsonPropertyName("scopes")]
    public string[] Scopes { get; set; }
}
