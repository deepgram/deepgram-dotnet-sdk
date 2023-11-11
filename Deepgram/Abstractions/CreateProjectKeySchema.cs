namespace Deepgram.Abstractions;
public abstract class CreateProjectKeySchema
{
    [JsonPropertyName("comment")]
    public string Comment { get; set; }

    [JsonPropertyName("scopes")]
    public string Scopes { get; set; }

    [JsonPropertyName("tags")]
    public string Tags { get; set; }

}
