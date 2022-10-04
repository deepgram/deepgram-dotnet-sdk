using Newtonsoft.Json;

namespace Deepgram.Projects
{
    public class ScopesList
    {
        /// <summary>
        /// Lists the specified project scopes assigned to the specified member
        /// </summary>
        [JsonProperty("scopes")]
        public string[] Scopes{ get; set; }
    }
}
