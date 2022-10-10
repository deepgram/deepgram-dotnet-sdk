using Newtonsoft.Json;

namespace Deepgram.Keys
{
    public class KeyMember
    {
        /// <summary>
        /// Unique identifier of member
        /// </summary>
        [JsonProperty("member_id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// First name of member
        /// </summary>
        [JsonProperty("first_name")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Last name of member
        /// </summary>
        [JsonProperty("last_name")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Email address of member
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;
    }
}
