﻿using SharpPulsar.Common.PulsarApi;
using System;
using System.Threading.Tasks;

namespace SharpPulsar.Impl.Internal
{
    public sealed class RequestResponseHandler : IDisposable
    {
        private const string ConnectResponseIdentifier = "Connected";

        private readonly Awaitor<string, BaseCommand> _responses;
        private ulong _requestId;

        public RequestResponseHandler()
        {
            _responses = new Awaitor<string, BaseCommand>();
            _requestId = 1;
        }

        public void Dispose() => _responses.Dispose();

        public Task<BaseCommand> Outgoing(BaseCommand command)
        {
            SetRequestId(command);
            return _responses.CreateTask(GetResponseIdentifier(command));
        }

        public void Incoming(BaseCommand command)
        {
            var identifier = GetResponseIdentifier(command);
            if (identifier != null)
                _responses.SetResult(identifier, command);
        }

        private void SetRequestId(BaseCommand cmd)
        {
            switch (cmd.type)
            {
                case BaseCommand.Type.Seek:
                    cmd.Seek.RequestId = _requestId++;
                    return;
                case BaseCommand.Type.Lookup:
                    cmd.lookupTopic.RequestId = _requestId++;
                    return;
                case BaseCommand.Type.Error:
                    cmd.Error.RequestId = _requestId++;
                    return;
                case BaseCommand.Type.Producer:
                    cmd.Producer.RequestId = _requestId++;
                    return;
                case BaseCommand.Type.CloseProducer:
                    cmd.CloseProducer.RequestId = _requestId++;
                    return;
                case BaseCommand.Type.Subscribe:
                    cmd.Subscribe.RequestId = _requestId++;
                    return;
                case BaseCommand.Type.Unsubscribe:
                    cmd.Unsubscribe.RequestId = _requestId++;
                    return;
                case BaseCommand.Type.CloseConsumer:
                    cmd.CloseConsumer.RequestId = _requestId++;
                    return;
                case BaseCommand.Type.GetLastMessageId:
                    cmd.getLastMessageId.RequestId = _requestId++;
                    return;
            }
        }

        private string GetResponseIdentifier(BaseCommand cmd)
        {
            switch (cmd.type)
            {
                case BaseCommand.Type.Connect:
                case BaseCommand.Type.Connected: return ConnectResponseIdentifier;
                case BaseCommand.Type.Send: return cmd.Send.ProducerId.ToString() + '-' + cmd.Send.SequenceId.ToString();
                case BaseCommand.Type.SendError: return cmd.SendError.ProducerId.ToString() + '-' + cmd.SendError.SequenceId.ToString();
                case BaseCommand.Type.SendReceipt: return cmd.SendReceipt.ProducerId.ToString() + '-' + cmd.SendReceipt.SequenceId.ToString();
                case BaseCommand.Type.Error: return _requestId == 1 ? ConnectResponseIdentifier : cmd.Error.RequestId.ToString();
                case BaseCommand.Type.Producer: return cmd.Producer.RequestId.ToString();
                case BaseCommand.Type.ProducerSuccess: return cmd.ProducerSuccess.RequestId.ToString();
                case BaseCommand.Type.CloseProducer: return cmd.CloseProducer.RequestId.ToString();
                case BaseCommand.Type.Lookup: return cmd.lookupTopic.RequestId.ToString();
                case BaseCommand.Type.LookupResponse: return cmd.lookupTopicResponse.RequestId.ToString();
                case BaseCommand.Type.Unsubscribe: return cmd.Unsubscribe.RequestId.ToString();
                case BaseCommand.Type.Subscribe: return cmd.Subscribe.RequestId.ToString();
                case BaseCommand.Type.Success: return cmd.Success.RequestId.ToString();
                case BaseCommand.Type.Seek: return cmd.Seek.RequestId.ToString();
                case BaseCommand.Type.CloseConsumer: return cmd.CloseConsumer.RequestId.ToString();
                case BaseCommand.Type.GetLastMessageId: return cmd.getLastMessageId.RequestId.ToString();
                case BaseCommand.Type.GetLastMessageIdResponse: return cmd.getLastMessageIdResponse.RequestId.ToString();
                default: throw new ArgumentOutOfRangeException("CommandType", cmd.type, "CommandType not supported as request/response type");
            }
        }
    }
}
