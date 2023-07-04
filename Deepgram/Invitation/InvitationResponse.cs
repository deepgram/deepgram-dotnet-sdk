using Newtonsoft.Json;

namespace Deepgram.Invitation
{

    public class InvitationResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
