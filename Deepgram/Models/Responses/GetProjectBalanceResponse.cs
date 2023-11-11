namespace Deepgram.Models.Responses;

public class GetProjectBalanceResponse
{
    [JsonPropertyName("balance_id")]
    public string? BalanceId { get; set; }

    [JsonPropertyName("amount")]
    public double? Amount { get; set; }

    [JsonPropertyName("units")]
    public string? Units { get; set; }

    [JsonPropertyName("purchase")]
    public string? Purchase { get; set; }
}
