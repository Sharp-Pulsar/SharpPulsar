
namespace Testcontainers.Pulsar
{
    /// <inheritdoc cref="ContainerConfiguration" />
    [PublicAPI]
    public sealed class PulsarConfiguration : ContainerConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PulsarConfiguration" /> class.
        /// </summary>
        public PulsarConfiguration()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PulsarConfiguration" /> class.
        /// </summary>
        /// <param name="resourceConfiguration">The Docker resource configuration.</param>
        public PulsarConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
            : base(resourceConfiguration)
        {
            // Passes the configuration upwards to the base implementations to create an updated immutable copy.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PulsarConfiguration" /> class.
        /// </summary>
        /// <param name="resourceConfiguration">The Docker resource configuration.</param>
        public PulsarConfiguration(IContainerConfiguration resourceConfiguration)
            : base(resourceConfiguration)
        {
            // Passes the configuration upwards to the base implementations to create an updated immutable copy.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PulsarConfiguration" /> class.
        /// </summary>
        /// <param name="resourceConfiguration">The Docker resource configuration.</param>
        public PulsarConfiguration(PulsarConfiguration resourceConfiguration)
            
        {
            // Passes the configuration upwards to the base implementations to create an updated immutable copy.
        }
        
    }
}
