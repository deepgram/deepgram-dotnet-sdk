namespace Deepgram.Records;

public record IntentsInfo
{

    [JsonPropertyName("model_uuid")]
    public string ModelUuid { get; set; }


    [JsonPropertyName("input_tokens")]
    public int? InputTokens { get; set; }


    [JsonPropertyName("output_tokens")]
    public int? OutputTokens { get; set; }
}


