namespace Deepgram.Records;

public record GetProjectBalanceResponse
{
    /// <summary>
    /// Balance id
    /// </summary>
    [JsonPropertyName("balance_id")]
    public string BalanceId { get; set; }

    /// <summary>
    /// Balance amount
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Units of the balance. May use usd or hour, depending on the project billing settings
    /// </summary>
    [JsonPropertyName("units")]
    public string Units { get; set; }

    /// <summary>
    /// Unique identifier of the purchase order associated with the balance
    /// </summary>
    [JsonPropertyName("purchase")]
    public string Purchase { get; set; }
}
