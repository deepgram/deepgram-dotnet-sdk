namespace Deepgram.Models.Schemas;

public class CreateProjectKeyWIthTimeToLiveSchema : CreateProjectKeySchema
{
    /// <summary>
    /// Time for the key to live in seconds 
    /// </summary>
    [JsonPropertyName("time_to_live_in_seconds")]
    public int? TimeToLiveInSeconds { get; set; }
}

