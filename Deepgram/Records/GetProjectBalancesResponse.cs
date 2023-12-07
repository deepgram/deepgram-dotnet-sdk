namespace Deepgram.Records;

public record GetProjectBalancesResponse
{
    [JsonPropertyName("balances")]
    public IReadOnlyList<GetProjectBalanceResponse>? Balances { get; set; }
}
