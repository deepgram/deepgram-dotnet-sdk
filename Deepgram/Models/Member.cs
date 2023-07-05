namespace Deepgram.Models;

public class Member
{
    /// <summary>
    /// Unique identifier of member
    /// </summary>
    [JsonPropertyName("member_id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// First name of member
    /// </summary>
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name of member
    /// </summary>
    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Project scopes associated with member
    /// </summary>
    [JsonPropertyName("scopes")]
    public IEnumerable<string> Scopes { get; set; } = new List<string>();

    /// <summary>
    /// Email address of member
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}
