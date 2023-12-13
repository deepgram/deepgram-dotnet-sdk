namespace Deepgram.Models;
public class CreateProjectKeySchema
{

    public CreateProjectKeySchema(string comment, List<string> scopes)
    {
        Comment = comment;
        Scopes = scopes;
    }

    /// <summary>
    /// Comment to describe key
    /// </summary>
    [JsonPropertyName("comment")]
    public string Comment { get; set; }

    /// <summary>
    /// Scopes of the key
    /// </summary>
    [JsonPropertyName("scopes")]
    public List<string> Scopes { get; set; }

    /// <summary>
    /// Tag names for key
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Date on which the key should expire if you set this do not set the TimeToLiveInSeconds
    /// </summary>
    [JsonPropertyName("expiration_date")]
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Time for the key to live in seconds 
    /// if you se this do not set the ExpirationDate
    /// </summary>
    [JsonPropertyName("time_to_live_in_seconds")]
    public int? TimeToLiveInSeconds { get; set; }


}
