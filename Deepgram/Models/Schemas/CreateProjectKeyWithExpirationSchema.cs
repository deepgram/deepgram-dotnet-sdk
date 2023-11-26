namespace Deepgram.Models.Schemas;

public class CreateProjectKeyWithExpirationSchema : CreateProjectKeySchema
{
    /// <summary>
    /// Date on which the key should expire
    /// </summary>
    [JsonPropertyName("expiration_date")]
    public DateTime ExpirationDate { get; set; }


}

