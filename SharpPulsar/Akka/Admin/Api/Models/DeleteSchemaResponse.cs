// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace PulsarAdmin.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class DeleteSchemaResponse
    {
        /// <summary>
        /// Initializes a new instance of the DeleteSchemaResponse class.
        /// </summary>
        public DeleteSchemaResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the DeleteSchemaResponse class.
        /// </summary>
        public DeleteSchemaResponse(long? version = default(long?))
        {
            Version = version;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "version")]
        public long? Version { get; set; }

    }
}
