namespace Deepgram.Records;

public record Response
{
    /// <summary>
    /// Details of the request <see cref="Detail"/>
    /// </summary>
    [JsonPropertyName("details")]
    public Details? Details { get; set; }

    [JsonPropertyName("code")]
    public int? Code { get; set; }

    [JsonPropertyName("completed")]
    public string Completed { get; set; }
}
