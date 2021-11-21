using System;
using Newtonsoft.Json;

namespace Deepgram.Transcription
{
    public class UrlSource
    {
        public UrlSource(String url)
        {
            Url = url;
        }

        /// <summary>
        /// Url of the file to transcribe
        /// </summary>
        [JsonProperty("url")]
        public String Url { get; set; } = String.Empty;
    }
}
