namespace Deepgram.Models.Manage.v1;
public record Member
{
    /// <summary>
    /// Unique identifier of member
    /// </summary>
    [JsonPropertyName("member_id")]
    public string? MemberId { get; set; }

    /// <summary>
    /// First name of member
    /// </summary>
    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name of member
    /// </summary>
    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }


    /// <summary>
    /// Email address of member
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; } = string.Empty;


}
