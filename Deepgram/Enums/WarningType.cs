using System.Runtime.Serialization;

namespace Deepgram.Enums;

[DataContract]
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
