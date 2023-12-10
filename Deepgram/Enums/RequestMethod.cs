
using System.Runtime.Serialization;

namespace Deepgram.Enums;

[DataContract]
public enum RequestMethod
{
    /// <summary>
    /// Synchronous
    /// </summary> 
    [EnumMember(Value = "sync")]
    sync,
    /// <summary>
    /// Asynchronous
    /// </summary>
    [EnumMember(Value = "async")]
    async,
    /// <summary>
    /// Streaming
    /// </summary>
    [EnumMember(Value = "streaming")]
    streaming
}
