namespace Deepgram.Models;

public class BillingList
{
    /// <summary>
    /// List of balances
    /// </summary>
    [JsonPropertyName("balances")]
    public Billing[] Billings { get; set; }
}
