using System;
using System.IO;
using Newtonsoft.Json;

namespace Deepgram.Transcription
{
    
    public class StreamSource
    {
        public StreamSource(Stream stream, string mimetype)
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
        public string MimeType { get; set; } = string.Empty;
    }
}
