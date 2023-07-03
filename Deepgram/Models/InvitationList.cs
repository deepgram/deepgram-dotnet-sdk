namespace Deepgram.Models;

public class InvitationList
{
    [JsonProperty("invites")]
    public InvitationOptions[] Invites { get; set; }

    [JsonProperty("err_code")]
    public string ErrorCode { get; set; }

    [JsonProperty("err_msg")]
    public string ErrorMessage { get; set; }
}
