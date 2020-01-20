using SharpPulsar.Common.PulsarApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Command.Builder
{
    public class CommandReachedEndOfTopicBuilder
    {
        private readonly CommandReachedEndOfTopic _topic;
        public CommandReachedEndOfTopicBuilder()
        {
            _topic = new CommandReachedEndOfTopic();
        }
        public CommandReachedEndOfTopicBuilder(CommandReachedEndOfTopic topic)
        {
            _topic = topic;
        }
        public CommandReachedEndOfTopicBuilder SetConsumerId(long consumerid)
        {
            _topic.ConsumerId = (ulong)consumerid;
            return new CommandReachedEndOfTopicBuilder(_topic);
        }
        public CommandReachedEndOfTopic Build()
        {
            return _topic;
        }
    }
}
