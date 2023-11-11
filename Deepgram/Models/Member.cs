namespace Deepgram.Models;
public class Member
{
    [JsonPropertyName("member_id")]
    public string? MemberId { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("scopes")]
    public string[]? Scopes { get; set; }
}
