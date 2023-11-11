namespace Deepgram.Models.Schemas;
public class GetProjectUsageFieldsSchema
{
    [JsonPropertyName("start")]
    public string? Start { get; set; }

    [JsonPropertyName("end")]
    public string? End { get; set; }
}