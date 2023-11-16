namespace Deepgram.Models.Options;

public class ExpirationOptions : CreateProjectKeySchema
{
    [JsonPropertyName("expiration_date")]
    public DateTime? ExpirationDate { get; set; }


}

