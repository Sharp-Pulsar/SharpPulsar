
namespace SharpPulsar.Akka.Admin.Api.Models
{
    /**
 * Pulsar Schema compatibility strategy.
 */
    public enum SchemaCompatibilityStrategy
    {

        /**
         * Undefined.
         */
        UNDEFINED,

        /**
         * Always incompatible.
         */
        ALWAYS_INCOMPATIBLE,

        /**
         * Always compatible.
         */
        ALWAYS_COMPATIBLE,

        /**
         * Messages written by an old schema can be read by a new schema.
         */
        BACKWARD,

        /**
         * Messages written by a new schema can be read by an old schema.
         */
        FORWARD,

        /**
         * Equivalent to both FORWARD and BACKWARD.
         */
        FULL,

        /**
         * Be similar to BACKWARD, BACKWARD_TRANSITIVE ensure all previous version schema can
         * be read by the new schema.
         */
        BACKWARD_TRANSITIVE,

        /**
         * Be similar to FORWARD, FORWARD_TRANSITIVE ensure new schema can be ready by all previous
         * version schema.
         */
        FORWARD_TRANSITIVE,

        /**
         * Equivalent to both FORWARD_TRANSITIVE and BACKWARD_TRANSITIVE.
         */
        FULL_TRANSITIVE
    }
}

