using System;
using Newtonsoft.Json;

namespace Deepgram.Common
{
    public class MessageResponse
    {
        /// <summary>
        /// A message denoting the success of the operation
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;
    }
}
