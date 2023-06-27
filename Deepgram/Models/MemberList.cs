using Newtonsoft.Json;

namespace Deepgram.Models
{
    public class MemberList
    {
        /// <summary>
        /// List of members
        /// </summary>
        [JsonProperty("members")]
        public Member[] Members { get; set; }
    }
}
