namespace Deepgram.Models.Manage.v1;

public record MembersResponse
{
    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("members")]
    public IReadOnlyList<Member>? Members { get; set; }
}
