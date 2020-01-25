using SharpPulsar.Common.PulsarApi;
namespace SharpPulsar.Command.Builder
{
    public class CommandReachedEndOfTopicBuilder
    {
        private readonly CommandReachedEndOfTopic _topic;
        public CommandReachedEndOfTopicBuilder()
        {
            _topic = new CommandReachedEndOfTopic();
        }
        
        public CommandReachedEndOfTopicBuilder SetConsumerId(long consumerid)
        {
            _topic.ConsumerId = (ulong)consumerid;
            return this;
        }
        public CommandReachedEndOfTopic Build()
        {
            return _topic;
        }
    }
}
