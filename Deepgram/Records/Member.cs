namespace Deepgram.Records;
public record Member
{
    [JsonPropertyName("member_id")]
    public string? MemberId { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }


}
