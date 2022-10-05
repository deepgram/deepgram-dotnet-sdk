using Newtonsoft.Json;

namespace Deepgram.Projects
{
    public class UpdateScopeOptions
    {
        /// <summary>
        /// New scope
        /// </summary>
        [JsonProperty("scope")]
        public string Scope { get; set; }
    }
}
