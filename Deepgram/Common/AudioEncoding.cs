using System;

namespace Deepgram.Common
{
    /// <summary>
    /// Expected encoding of the submitted streaming audio.
    /// </summary>
    public static class AudioEncoding
    {
        /// <summary>
        /// 16-bit, little endian, signed PCM WAV data
        /// </summary>
        public static readonly string Linear16 = "linear16";
        /// <summary>
        /// FLAC-encoded data
        /// </summary>
        public static readonly string FLAC = "flac";
        /// <summary>
        /// mu-law encoded WAV data
        /// </summary>
        public static readonly string MuLaw = "mulaw";
        /// <summary>
        /// adaptive multi-rate narrowband codec (sample rate must be 8000)
        /// </summary>
        public static readonly string AMRNB = "amr-nb";
        /// <summary>
        /// adaptive multi-rate wideband codec (sample rate must be 16000)
        /// </summary>
        public static readonly string AMRWB = "amr-wb";
        /// <summary>
        /// Ogg Opus
        /// </summary>
        public static readonly string OggOpus = "opus";
        /// <summary>
        /// Ogg Speex
        /// </summary>
        public static readonly string OggSpeex = "speex";
    }
}
