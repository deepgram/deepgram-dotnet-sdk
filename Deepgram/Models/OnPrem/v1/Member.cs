namespace Deepgram.Models.OnPrem.v1;

public record Member
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("member_id")]
    public string? MemberId { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }
}
