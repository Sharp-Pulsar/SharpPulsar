
using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Consumer
{
    public readonly record struct SeekMessageId(IMessageId MessageId);
}
