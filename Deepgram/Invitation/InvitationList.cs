using Newtonsoft.Json;

namespace Deepgram.Invitation
{

    public class InvitationList
    {
        /// <summary>
        /// Options for Invitation
        /// </summary>
        [JsonProperty("invites")]
        public InvitationOptions[] Invites { get; set; }

        [JsonProperty("err_code")]
        public string ErrorCode { get; set; }

        [JsonProperty("err_msg")]
        public string ErrorMessage { get; set; }
    }
}
