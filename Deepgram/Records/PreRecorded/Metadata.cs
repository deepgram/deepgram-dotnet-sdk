namespace Deepgram.Records.PreRecorded;
public record Metadata
{

    /// <summary>
    /// Timestamp that indicates when the audio was submitted.
    /// </summary>
    [JsonPropertyName("created")]
    public DateTime? Created { get; set; }

    /// <summary>
    /// Number of channels detected in the submitted audio.
    /// </summary>
    [JsonPropertyName("channels")]
    public int? Channels { get; set; }

    /// <summary>
    /// Duration in seconds of the submitted audio.
    /// </summary>
    [JsonPropertyName("duration")]
    public decimal? Duration { get; set; }

    /// <summary>
    /// IReadonlyDictionary of <see cref="ModelInfo"/>
    /// </summary>
    [JsonPropertyName("model_info")]
    public Dictionary<string, ModelInfo> ModelInfo { get; set; }


    [JsonPropertyName("models")]
    public List<string>? Models { get; set; }

    /// <summary>
    /// Unique identifier for the submitted audio and derived data returned.
    /// </summary>
    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }

    /// <summary>
    /// SHA-256 hash of the submitted audio data.
    /// </summary>
    [JsonPropertyName("sha256")]
    public string? Sha256 { get; set; }

    /// <summary>
    /// Tags relating to the project
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Blob of text that helps Deepgram engineers debug any problems you encounter.
    /// 
    /// </summary>
    [JsonPropertyName("transaction_key")]
    [Obsolete]
    public string? TransactionKey { get; set; }

    [JsonPropertyName("sentiment_info")]
    public SentimentInfo SentimentInfo { get; set; }

    [JsonPropertyName("intents_info")]
    public IntentsInfo IntentsInfo { get; set; }

    /// <summary>
    /// Warnings to provide feedback about unsupported and deprecated queries.
    /// </summary>
    [JsonPropertyName("warnings")]
    public List<Warning>? Warnings { get; set; }


}
