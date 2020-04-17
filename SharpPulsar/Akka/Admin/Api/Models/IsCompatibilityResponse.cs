// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace PulsarAdmin.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class IsCompatibilityResponse
    {
        /// <summary>
        /// Initializes a new instance of the IsCompatibilityResponse class.
        /// </summary>
        public IsCompatibilityResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the IsCompatibilityResponse class.
        /// </summary>
        public IsCompatibilityResponse(string schemaCompatibilityStrategy = default(string), bool? compatibility = default(bool?))
        {
            SchemaCompatibilityStrategy = schemaCompatibilityStrategy;
            Compatibility = compatibility;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "schemaCompatibilityStrategy")]
        public string SchemaCompatibilityStrategy { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "compatibility")]
        public bool? Compatibility { get; set; }

    }
}
