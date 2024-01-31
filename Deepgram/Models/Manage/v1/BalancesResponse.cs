namespace Deepgram.Models.Manage.v1;

public record BalancesResponse
{
    /// <summary>
    /// ReadOnlyList of project balances <see cref="BalanceResponse"/>
    /// </summary>
    [JsonPropertyName("balances")]
    public IReadOnlyList<BalanceResponse>? Balances { get; set; }
}
