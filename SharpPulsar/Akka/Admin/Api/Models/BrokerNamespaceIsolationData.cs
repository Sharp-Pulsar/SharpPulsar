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

    /// <summary>
    /// The namespace isolation data for a given broker
    /// </summary>
    public partial class BrokerNamespaceIsolationData
    {
        /// <summary>
        /// Initializes a new instance of the BrokerNamespaceIsolationData
        /// class.
        /// </summary>
        public BrokerNamespaceIsolationData()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the BrokerNamespaceIsolationData
        /// class.
        /// </summary>
        /// <param name="brokerName">The broker name</param>
        /// <param name="policyName">Policy name</param>
        /// <param name="isPrimary">Is Primary broker</param>
        /// <param name="namespaceRegex">The namespace-isolation policies
        /// attached to this broker</param>
        public BrokerNamespaceIsolationData(string brokerName = default(string), string policyName = default(string), bool? isPrimary = default(bool?), IList<string> namespaceRegex = default(IList<string>))
        {
            BrokerName = brokerName;
            PolicyName = policyName;
            IsPrimary = isPrimary;
            NamespaceRegex = namespaceRegex;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets the broker name
        /// </summary>
        [JsonProperty(PropertyName = "brokerName")]
        public string BrokerName { get; set; }

        /// <summary>
        /// Gets or sets policy name
        /// </summary>
        [JsonProperty(PropertyName = "policyName")]
        public string PolicyName { get; set; }

        /// <summary>
        /// Gets or sets is Primary broker
        /// </summary>
        [JsonProperty(PropertyName = "isPrimary")]
        public bool? IsPrimary { get; set; }

        /// <summary>
        /// Gets or sets the namespace-isolation policies attached to this
        /// broker
        /// </summary>
        [JsonProperty(PropertyName = "namespaceRegex")]
        public IList<string> NamespaceRegex { get; set; }

    }
}
