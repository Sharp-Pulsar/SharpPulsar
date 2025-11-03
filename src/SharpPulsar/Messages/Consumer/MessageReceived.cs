using System.Buffers;
using DotNetty.Buffers;
using Pulsar.Proto;
using SharpPulsar.Client;

namespace SharpPulsar.Messages.Consumer
{
    public record MessageReceived(CommandMessage Message, AbstractByteBuffer Payload, ClientCnx ClientCnx);
}
