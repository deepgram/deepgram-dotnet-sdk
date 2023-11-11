namespace Deepgram.Models.Options;

public class TimeToLiveOptions : CreateProjectKeySchema
{
    [JsonPropertyName("time_to_live_in_seconds")]
    public int? TimeToLiveInSeconds { get; set; }
}

