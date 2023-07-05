namespace Deepgram.Models;

public class InvitationList
{
    /// <summary>
    /// Options for Invitation
    /// </summary>
    [JsonPropertyName("invites")]
    public InvitationOptions[] Invites { get; set; }

    [JsonPropertyName("err_code")]
    public string ErrorCode { get; set; }

    [JsonPropertyName("err_msg")]
    public string ErrorMessage { get; set; }
}
