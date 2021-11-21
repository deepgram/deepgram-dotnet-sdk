using System;
using System.Text.Json.Serialization;

namespace Deepgram.Common
{
    public class MessageResponse
    {
        /// <summary>
        /// A message denoting the success of the operation
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
