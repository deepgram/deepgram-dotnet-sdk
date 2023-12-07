namespace Deepgram.Models;
public class GetProjectUsageFieldsSchema
{
    [JsonPropertyName("start")]
    public DateTime? Start { get; set; }

    [JsonPropertyName("end")]
    public DateTime? End { get; set; }
}
