namespace Deepgram.Models.Schemas;

public class LiveSchema : TranscriptionSchema
{
    [JsonPropertyName("channels")]
    public int? Channels { get; set; }

    [JsonPropertyName("encoding")]
    public string? Encoding { get; set; }

    [JsonPropertyName("sample_rate")]
    public int? SampleRate { get; set; }

    [JsonPropertyName("endpointing")]
    public int? EndPointing { get; set; }

    [JsonPropertyName("interim_results")]
    public bool? InterimResults { get; set; }
}
