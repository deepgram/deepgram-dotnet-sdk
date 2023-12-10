namespace Deepgram.Records;

public record GetProjectBalancesResponse
{
    /// <summary>
    /// ReadOnlyList of project balances <see cref="GetProjectBalanceResponse"/>
    /// </summary>
    [JsonPropertyName("balances")]
    public IReadOnlyList<GetProjectBalanceResponse>? Balances { get; set; }
}
