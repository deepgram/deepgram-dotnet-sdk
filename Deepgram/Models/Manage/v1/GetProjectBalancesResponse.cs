namespace Deepgram.Models.Manage.v1;

public record GetProjectBalancesResponse
{
    /// <summary>
    /// ReadOnlyList of project balances <see cref="GetProjectBalanceResponse"/>
    /// </summary>
    [JsonPropertyName("balances")]
    public IReadOnlyList<GetProjectBalanceResponse>? Balances { get; set; }
}
