using System;
using System.Runtime.Serialization;

namespace Deepgram.Transcription
{
    public enum WarningType

    {
        [EnumMember(Value = "unsupported_language")]
        UnsupportedLanguage,
        
        [EnumMember(Value = "unsupported_model")]
        UnsupportedModel,
        
        [EnumMember(Value = "unsupported_encoding")]
        UnsupportedEncoding,
        
        [EnumMember(Value = "deprecated")]
        Deprecated
    }
}


