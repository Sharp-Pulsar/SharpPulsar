// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace PulsarAdmin.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class BookieInfo
    {
        /// <summary>
        /// Initializes a new instance of the BookieInfo class.
        /// </summary>
        public BookieInfo()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the BookieInfo class.
        /// </summary>
        public BookieInfo(string rack = default(string), string hostname = default(string))
        {
            Rack = rack;
            Hostname = hostname;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "rack")]
        public string Rack { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "hostname")]
        public string Hostname { get; set; }

    }
}
