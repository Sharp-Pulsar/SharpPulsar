
using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Consumer
{
    public record struct NegativeAcknowledgeMessage<T>(IMessage<T> Message);
   
    public record struct NegativeAcknowledgeMessages<T>(IMessages<T> Messages);
   
    public record struct NegativeAcknowledgeMessageId(IMessageId MessageId);
}
