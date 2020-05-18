using Newtonsoft.Json;

namespace SharpPulsar.Akka.Admin.Api.Models
{
    public partial class DelayedDeliveryPolicies
    {
        public DelayedDeliveryPolicies()
        {
            CustomInit();
        }
        public DelayedDeliveryPolicies(long tickTime = default(long), bool active = default(bool))
        {
            TickTime = tickTime;
            Active = active;
            CustomInit();
        }
        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();
        [JsonProperty(PropertyName = "tickTime")]
        public long TickTime { get; set; }

        [JsonProperty(PropertyName = "active")]
        public bool Active { get; set; }
    }
}
