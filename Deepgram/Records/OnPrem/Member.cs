namespace Deepgram.Records.OnPrem;
public record Member
{
    [JsonPropertyName("member_id")]
    public string? MemberId { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}
