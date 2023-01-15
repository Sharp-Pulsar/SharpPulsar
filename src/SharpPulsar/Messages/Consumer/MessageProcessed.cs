
using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Consumer
{   
    public record struct MessageProcessed<T>(IMessage<T> Message);
}
