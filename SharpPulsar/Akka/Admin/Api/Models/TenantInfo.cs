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
    /// Information of adminRoles and allowedClusters for tenant
    /// </summary>
    public partial class TenantInfo
    {
        /// <summary>
        /// Initializes a new instance of the TenantInfo class.
        /// </summary>
        public TenantInfo()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the TenantInfo class.
        /// </summary>
        /// <param name="adminRoles">Comma separated list of auth principal
        /// allowed to administrate the tenant.</param>
        /// <param name="allowedClusters">Comma separated allowed
        /// clusters.</param>
        public TenantInfo(IList<string> adminRoles = default(IList<string>), IList<string> allowedClusters = default(IList<string>))
        {
            AdminRoles = adminRoles;
            AllowedClusters = allowedClusters;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets comma separated list of auth principal allowed to
        /// administrate the tenant.
        /// </summary>
        [JsonProperty(PropertyName = "adminRoles")]
        public IList<string> AdminRoles { get; set; }

        /// <summary>
        /// Gets or sets comma separated allowed clusters.
        /// </summary>
        [JsonProperty(PropertyName = "allowedClusters")]
        public IList<string> AllowedClusters { get; set; }

    }
}
