namespace Deepgram.Models;

public class MemberList
{
    /// <summary>
    /// List of members
    /// </summary>
    [JsonPropertyName("members")]
    public Member[] Members { get; set; }
}
