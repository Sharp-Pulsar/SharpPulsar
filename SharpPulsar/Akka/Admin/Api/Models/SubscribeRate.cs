// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace PulsarAdmin.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class SubscribeRate
    {
        /// <summary>
        /// Initializes a new instance of the SubscribeRate class.
        /// </summary>
        public SubscribeRate()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the SubscribeRate class.
        /// </summary>
        public SubscribeRate(int? subscribeThrottlingRatePerConsumer = default(int?), int? ratePeriodInSecond = default(int?))
        {
            SubscribeThrottlingRatePerConsumer = subscribeThrottlingRatePerConsumer;
            RatePeriodInSecond = ratePeriodInSecond;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "subscribeThrottlingRatePerConsumer")]
        public int? SubscribeThrottlingRatePerConsumer { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "ratePeriodInSecond")]
        public int? RatePeriodInSecond { get; set; }

    }
}
