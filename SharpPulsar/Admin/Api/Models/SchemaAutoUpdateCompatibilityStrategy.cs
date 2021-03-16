namespace SharpPulsar.Admin.Api.Models
{
    /**
    * Strategy to use when checking an auto-updated schema for compatibility to the current schema.
    */
    public enum SchemaAutoUpdateCompatibilityStrategy
    {
        /**
         * Don't allow any auto updates.
         */
        AutoUpdateDisabled,

        /**
         * Messages written in the previous schema can be read by the new schema.
         * To be backward compatible, the new schema must not add any new fields that
         * don't have default values. However, it may remove fields.
         */
        Backward,

        /**
         * Messages written in the new schema can be read by the previous schema.
         * To be forward compatible, the new schema must not remove any fields which
         * don't have default values in the previous schema. However, it may add new fields.
         */
        Forward,

        /**
         * Backward and Forward.
         */
        Full,

        /**
         * Always Compatible - The new schema will not be checked for compatibility against
         * old schemas. In other words, new schemas will always be marked assumed compatible.
         */
        AlwaysCompatible,

        /**
         * Be similar to Backward. BackwardTransitive ensure all previous version schema can
         * be read by the new schema.
         */
        BackwardTransitive,

        /**
         * Be similar to Forward, ForwardTransitive ensure new schema can be ready by all previous
         * version schema.
         */
        ForwardTransitive,

        /**
         * BackwardTransitive and ForwardTransitive.
         */
        FullTransitive
    }
}
