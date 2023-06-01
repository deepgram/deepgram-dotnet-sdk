using System.Collections.Generic;
using Newtonsoft.Json;

namespace Deepgram.Models
{
    public class Member
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
        /// Project scopes associated with member
        /// </summary>
        [JsonProperty("scopes")]
        public IEnumerable<string> Scopes { get; set; } = new List<string>();

        /// <summary>
        /// Email address of member
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;
    }
}
