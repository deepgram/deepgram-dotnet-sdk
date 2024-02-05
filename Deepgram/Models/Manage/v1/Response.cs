namespace Deepgram.Models.Manage.v1;

public record Response
{
    /// <summary>
    /// Details of the request <see cref="Detail"/>
    /// </summary>
    [JsonPropertyName("details")]
    public Details? Details { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("code")]
    public int? Code { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("completed")]
    public string? Completed { get; set; }

    /// <summary>
    /// TODO
    /// </summary>
    [JsonPropertyName("token_details")]
    public List<TokenDetails>? TokenDetails { get; set; }
}
