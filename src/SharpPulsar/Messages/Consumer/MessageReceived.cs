using System.Buffers;
using DotNetty.Buffers;
using Pulsar.Proto;
using SharpPulsar.Client;
using static SharpPulsar.Client.ClientCnx;

namespace SharpPulsar.Messages.Consumer
{
    public record MessageReceived(CommandMessage Message, AbstractByteBuffer Payload, ClientHandler ClientCnx);
}
