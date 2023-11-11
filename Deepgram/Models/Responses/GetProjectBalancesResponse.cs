namespace Deepgram.Models.Responses;

public class GetProjectBalancesResponse
{
    [JsonPropertyName("balances")]
    public GetProjectBalanceResponse[]? Balances { get; set; }
}
