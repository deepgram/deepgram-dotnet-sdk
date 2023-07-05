namespace Deepgram.Models;

public class KeyMember
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
    /// Email address of member
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}
