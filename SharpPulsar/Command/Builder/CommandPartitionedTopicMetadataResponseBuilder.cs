using SharpPulsar.Common.PulsarApi;
using System;
using System.Collections.Generic;
using System.Text;
using static SharpPulsar.Common.PulsarApi.CommandLookupTopicResponse;

namespace SharpPulsar.Command.Builder
{
    public class CommandPartitionedTopicMetadataResponseBuilder
    {
        private readonly CommandPartitionedTopicMetadataResponse _response;
        public CommandPartitionedTopicMetadataResponseBuilder()
        {
            _response = new CommandPartitionedTopicMetadataResponse();
        }
        private CommandPartitionedTopicMetadataResponseBuilder(CommandPartitionedTopicMetadataResponse response)
        {
            _response = response;
        }
        public CommandPartitionedTopicMetadataResponseBuilder SetError(ServerError error)
        {
            _response.Error = error;
            return new CommandPartitionedTopicMetadataResponseBuilder(_response);
        }
        public CommandPartitionedTopicMetadataResponseBuilder SetRequestId(long requestid)
        {
            _response.RequestId = (ulong)requestid;
            return new CommandPartitionedTopicMetadataResponseBuilder(_response);
        }
        public CommandPartitionedTopicMetadataResponseBuilder SetMessage(string message)
        {
            _response.Message = message;
            return new CommandPartitionedTopicMetadataResponseBuilder(_response);
        }
        public CommandPartitionedTopicMetadataResponseBuilder SetPartitions(int partitions)
        {
            _response.Partitions = (uint)partitions;
            return new CommandPartitionedTopicMetadataResponseBuilder(_response);
        }
        public CommandPartitionedTopicMetadataResponseBuilder SetResponse(CommandPartitionedTopicMetadataResponse.LookupType type)
        {
            _response.Response = type;
            return new CommandPartitionedTopicMetadataResponseBuilder(_response);
        }
        public CommandPartitionedTopicMetadataResponse Build()
        {
            return _response;
        }
    }
}
