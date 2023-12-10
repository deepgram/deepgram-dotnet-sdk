namespace Deepgram.Records.PreRecorded;

public record Warning
{
    /// <summary>
    /// Parameter sent in the request that resulted in the warning
    /// </summary>
    [JsonPropertyName("parameter")]
    public string Parameter { get; set; }

    /// <summary>
    /// The type of warning
    /// </summary>
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WarningType Type { get; set; }

    /// <summary>
    /// The warning message
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }
}

