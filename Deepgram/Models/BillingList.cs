using Newtonsoft.Json;

namespace Deepgram.Models
{
    public class BillingList
    {
        /// <summary>
        /// List of balances
        /// </summary>
        [JsonProperty("balances")]
        public Billing[] Billings { get; set; }
    }
}
