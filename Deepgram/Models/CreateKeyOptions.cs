namespace Deepgram.Models;

public class CreateKeyOptions
{
    /// <summary>
    /// Date on which the key you would like to create should expire.
    /// </summary>
    [JsonPropertyName("expiration_date")]
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Length of time (in seconds) during which the key you would like to create will remain valid.   
    /// </summary>
    [JsonPropertyName("time_to_live_in_seconds")]
    public int? TimeToLive { get; set; }

    /// <summary>
    ///   Tags associated with the key you would like to create
    /// </summary
    [JsonPropertyName("tags")]
    public string[]? Tags { get; set; }


}
