using Newtonsoft.Json;

namespace Deepgram.Projects
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
