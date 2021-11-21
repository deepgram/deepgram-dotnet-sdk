using System;
using Newtonsoft.Json;

namespace Deepgram.Transcription
{
    public class Channel
    {
        /// <summary>
        /// Array of Search objects.
        /// </summary>
        [JsonProperty("search")]
        public Search[]? Search { get; set; }

        /// <summary>
        /// Array of Alternative objects.
        /// </summary>
        [JsonProperty("alternatives")]
        public Alternative[] Alternatives { get; set; }

    }
}
