using System;
using Newtonsoft.Json;

namespace Deepgram.Models
{
    public class Translation
    {
        /// <summary>
        /// Translated transcript.
        /// </summary>
        [JsonProperty("translation")]
        public string TranslatedTranscript { get; set; }

        /// <summary>
        /// Language code of the translation.
        /// </summary>
        [JsonProperty("language")]
        public string Language { get; set; }
    }
}
