// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace PulsarAdmin.Models
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class Metrics
    {
        /// <summary>
        /// Initializes a new instance of the Metrics class.
        /// </summary>
        public Metrics()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the Metrics class.
        /// </summary>
        public Metrics(IDictionary<string, object> metricsProperty = default(IDictionary<string, object>), IDictionary<string, string> dimensions = default(IDictionary<string, string>))
        {
            MetricsProperty = metricsProperty;
            Dimensions = dimensions;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "metrics")]
        public IDictionary<string, object> MetricsProperty { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "dimensions")]
        public IDictionary<string, string> Dimensions { get; set; }

    }
}
