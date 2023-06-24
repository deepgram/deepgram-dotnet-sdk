using System;

namespace Deepgram.Models
{
    public enum RequestMethod
    {
        /// <summary>
        /// Synchronous
        /// </summary>
        sync,
        /// <summary>
        /// Asynchronus
        /// </summary>
        async,
        /// <summary>
        /// Streaming
        /// </summary>
        streaming
    }
}
