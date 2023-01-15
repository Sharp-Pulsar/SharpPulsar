using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Client
{
    public record struct PreProcessedSchema<T>(ISchema<T> Schema);
}
