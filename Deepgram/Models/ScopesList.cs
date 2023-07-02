namespace Deepgram.Models;

public class ScopesList
{
    /// <summary>
    /// Lists the specified project scopes assigned to the specified member
    /// </summary>
    [JsonProperty("scopes")]
    public string[] Scopes { get; set; }
}
