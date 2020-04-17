// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace PulsarAdmin.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class SystemResourceUsage
    {
        /// <summary>
        /// Initializes a new instance of the SystemResourceUsage class.
        /// </summary>
        public SystemResourceUsage()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the SystemResourceUsage class.
        /// </summary>
        public SystemResourceUsage(ResourceUsage bandwidthIn = default(ResourceUsage), ResourceUsage bandwidthOut = default(ResourceUsage), ResourceUsage cpu = default(ResourceUsage), ResourceUsage memory = default(ResourceUsage), ResourceUsage directMemory = default(ResourceUsage))
        {
            BandwidthIn = bandwidthIn;
            BandwidthOut = bandwidthOut;
            Cpu = cpu;
            Memory = memory;
            DirectMemory = directMemory;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "bandwidthIn")]
        public ResourceUsage BandwidthIn { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "bandwidthOut")]
        public ResourceUsage BandwidthOut { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "cpu")]
        public ResourceUsage Cpu { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "memory")]
        public ResourceUsage Memory { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "directMemory")]
        public ResourceUsage DirectMemory { get; set; }

    }
}
