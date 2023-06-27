using Newtonsoft.Json;

namespace Deepgram.Models
{
    public class Billing
    {
        /// <summary>
        /// Unique identifier of the balance
        /// </summary>
        [JsonProperty("balance_id")]
        public string BalanceId { get; set; } = string.Empty;

        /// <summary>
        /// Amount of the balance
        /// </summary>
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Units of the balance. May use usd or hour, depending on the project billing settings
        /// </summary>
        [JsonProperty("units")]
        public string Units { get; set; }

        /// <summary>
        /// Unique identifier of the purchase order associated with the balance
        /// </summary>
        [JsonProperty("purchase")]
        public string Purchase { get; set; }
    }
}
