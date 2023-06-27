using Newtonsoft.Json;
using System;

namespace Deepgram.Models
{
    public class UrlSource
    {
        public UrlSource(string url)
        {
            Url = url;
        }

        /// <summary>
        /// Url of the file to transcribe
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;
    }
}
