namespace Deepgram.Models.PreRecorded.v1;

public record Metadata
{
    /// <summary>
    /// Number of channels detected in the submitted audio.
    /// </summary>
    [JsonPropertyName("channels")]
    public int? Channels { get; set; }

    /// <summary>
    /// Timestamp that indicates when the audio was submitted.
    /// </summary>
    [JsonPropertyName("created")]
    public DateTime? Created { get; set; }

    /// <summary>
    /// Duration in seconds of the submitted audio.
    /// </summary>
    [JsonPropertyName("duration")]
    public double? Duration { get; set; }

    /// <summary>
    /// Deepgram’s Extra Metadata feature allows you to attach arbitrary key-value pairs to your API requests that are attached to the API response for usage in downstream processing.
    /// Extra metadata is limited to 2048 characters per key-value pair.
    /// <see href="https://developers.deepgram.com/docs/extra-metadata"/>
    /// </summary>
    [JsonPropertyName("extra")]
    public Dictionary<string, string>? Extra { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("intents_info")]
    public IntentsInfo? IntentsInfo { get; set; }

    /// <summary>
    /// IReadonlyDictionary of <see cref="ModelInfo"/>
    /// </summary>
    [JsonPropertyName("model_info")]
    public Dictionary<string, ModelInfo>? ModelInfo { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("models")]
    public List<string>? Models { get; set; }

    /// <summary>
    /// Unique identifier for the submitted audio and derived data returned.
    /// </summary>
    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }

    /// <summary>
    /// TODO.
    /// </summary>
    [JsonPropertyName("sentiment_info")]
    public SentimentInfo? SentimentInfo { get; set; }

    /// <summary>
    /// SHA-256 hash of the submitted audio data.
    /// </summary>
    [JsonPropertyName("sha256")]
    public string? Sha256 { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("summary_info")]
    public SummaryInfo? SummaryInfo { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("topics_info")]
    public TopicsInfo? TopicsInfo { get; set; }

    /// <summary>
    /// Blob of text that helps Deepgram engineers debug any problems you encounter.
    /// </summary>
    [JsonPropertyName("transaction_key")]
    [Obsolete("phasing out")]
    public string? TransactionKey { get; set; }

    /// <summary>
    /// Warnings to provide feedback about unsupported and deprecated queries.
    /// </summary>
    [JsonPropertyName("warnings")]
    public List<Warning>? Warnings { get; set; }
}
