using System;
using Newtonsoft.Json;

namespace Deepgram.Transcription
{
    
    public class StreamSource
    {
        public StreamSource(Stream stream, String mimetype)
        {
            Stream = stream;
            MimeType = mimetype;
        }

        /// <summary>
        /// Stream to transcribe
        /// </summary>
        public Stream Stream { get; set; }

        /// <summary>
        /// MIMETYPE of the stream
        /// </summary>
        public String MimeType { get; set; } = string.Empty;
    }
}
