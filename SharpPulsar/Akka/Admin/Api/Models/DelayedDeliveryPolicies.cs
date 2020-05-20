using Newtonsoft.Json;

namespace SharpPulsar.Akka.Admin.Api.Models
{
    public partial class DelayedDeliveryPolicies
    {
        /// <summary>
        /// Initializes a new instance of the DelayedDeliveryPolicies class.
        /// </summary>
        public DelayedDeliveryPolicies()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the DelayedDeliveryPolicies class.
        /// </summary>
        public DelayedDeliveryPolicies(long? tickTime = default(long?), bool? active = default(bool?))
        {
            TickTime = tickTime;
            Active = active;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "tickTime")]
        public long? TickTime { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "active")]
        public bool? Active { get; set; }

    }
}
