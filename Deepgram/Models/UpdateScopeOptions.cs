using Newtonsoft.Json;

namespace Deepgram.Models
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
