// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace PulsarAdmin.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class BacklogQuota
    {
        /// <summary>
        /// Initializes a new instance of the BacklogQuota class.
        /// </summary>
        public BacklogQuota()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the BacklogQuota class.
        /// </summary>
        /// <param name="policy">Possible values include:
        /// 'producer_request_hold', 'producer_exception',
        /// 'consumer_backlog_eviction'</param>
        public BacklogQuota(long? limit = default(long?), string policy = default(string))
        {
            Limit = limit;
            Policy = policy;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "limit")]
        public long? Limit { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'producer_request_hold',
        /// 'producer_exception', 'consumer_backlog_eviction'
        /// </summary>
        [JsonProperty(PropertyName = "policy")]
        public string Policy { get; set; }

    }
}
