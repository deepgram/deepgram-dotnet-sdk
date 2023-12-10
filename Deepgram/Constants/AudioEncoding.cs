namespace Deepgram.Constants;

/// <summary> 
/// Expected encoding of the submitted streaming audio. 
/// </summary> 
public static class AudioEncoding
{
    /// <summary> 
    /// 16-bit, little endian, signed PCM WAV data 
    /// </summary> 
    public const string Linear16 = "linear16";
    /// <summary> 
    /// FLAC-encoded data 
    /// </summary> 
    public const string FLAC = "flac";
    /// <summary> 
    /// mu-law encoded WAV data 
    /// </summary> 
    public const string MuLaw = "mulaw";
    /// <summary> 
    /// adaptive multi-rate narrowband codec (sample rate must be 8000) 
    /// </summary> 
    public const string AMRNB = "amr-nb";
    /// <summary> 
    /// adaptive multi-rate wideband codec (sample rate must be 16000) 
    /// </summary> 
    public const string AMRWB = "amr-wb";
    /// <summary> 
    /// Ogg Opus 
    /// </summary> 
    public const string OggOpus = "opus";
    /// <summary> 
    /// Ogg Speex 
    /// </summary> 
    public const string OggSpeex = "speex";
}

