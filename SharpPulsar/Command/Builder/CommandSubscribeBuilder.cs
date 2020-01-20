using SharpPulsar.Common.PulsarApi;
using SharpPulsar.Common.Schema;
using SharpPulsar.Entity;
using System;
using System.Collections.Generic;
using static SharpPulsar.Common.PulsarApi.CommandSubscribe;

namespace SharpPulsar.Command.Builder
{
    public class CommandSubscribeBuilder
    {
        private readonly CommandSubscribe _subscribe;
        public CommandSubscribeBuilder()
        {
            _subscribe = new CommandSubscribe();
        }
        private CommandSubscribeBuilder(CommandSubscribe subscribe)
        {
            _subscribe = subscribe;
        }
        public CommandSubscribeBuilder SetConsumerId(long consumerId)
        {
            _subscribe.ConsumerId = (ulong)consumerId;
            return new CommandSubscribeBuilder(_subscribe);
        }
        public CommandSubscribeBuilder SetConsumerName(string consumerName)
        {
            _subscribe.ConsumerName = consumerName;
            return new CommandSubscribeBuilder(_subscribe);
        }
        public CommandSubscribeBuilder SetDurable(bool isDurable)
        {
            _subscribe.Durable = isDurable;
            return new CommandSubscribeBuilder(_subscribe);
        }
        public CommandSubscribeBuilder SetForceTopicCreation(bool forceTopicCreation)
        {
            _subscribe.ForceTopicCreation = forceTopicCreation;
            return new CommandSubscribeBuilder(_subscribe);
        }
        public CommandSubscribeBuilder SetInitialPosition(InitialPosition position)
        {
            _subscribe.initialPosition = position;
            return new CommandSubscribeBuilder(_subscribe);
        }
        public CommandSubscribeBuilder SetkeySharedMeta(KeySharedPolicy keySharedPolicy)
        {
            if(keySharedPolicy != null)
            {
                switch (keySharedPolicy.KeySharedMode)
                {
                    case Enum.KeySharedMode.AUTO_SPLIT:
                        {
                            var keySharedData = new KeySharedMeta
                            {
                                keySharedMode = KeySharedMode.AutoSplit
                            };
                            _subscribe.keySharedMeta = keySharedData;
                        }
                        break;
                    case Enum.KeySharedMode.STICKY:
                        KeySharedMeta sharedMeta = new KeySharedMeta { keySharedMode = KeySharedMode.Sticky };
                        IList<Entity.Range> ranges = ((KeySharedPolicy.KeySharedPolicySticky)keySharedPolicy).GetRanges;
                        var intRanges = new List<IntRange>();
                        foreach (Entity.Range range in ranges)
                        {
                            intRanges.Add(new IntRange { Start = (range.Start), End = (range.End) });
                        }
                        //sharedMeta.hashRanges = intRanges;
                        _subscribe.keySharedMeta = sharedMeta;
                        break;
                }
            }
            return new CommandSubscribeBuilder(_subscribe);
        }
        public CommandSubscribeBuilder SetPriorityLevel(int priorityLevel)
        {
            _subscribe.PriorityLevel = priorityLevel;
            return new CommandSubscribeBuilder(_subscribe);
        }
        public CommandSubscribeBuilder SetReadCompacted(bool readCompacted)
        {
            _subscribe.ReadCompacted = readCompacted;
            return new CommandSubscribeBuilder(_subscribe);
        }
        public CommandSubscribeBuilder SetReplicateSubscriptionState(bool replicate)
        {
            _subscribe.ReplicateSubscriptionState = replicate;
            return new CommandSubscribeBuilder(_subscribe);
        }
        public CommandSubscribeBuilder SetRequestId(long requestid)
        {
            _subscribe.RequestId = (ulong)requestid;
            return new CommandSubscribeBuilder(_subscribe);
        }
        public CommandSubscribeBuilder SetSchema(SchemaInfo schemaInfo)
        {
            _subscribe.Schema = GetSchema(schemaInfo);
            return new CommandSubscribeBuilder(_subscribe);
        }
        public CommandSubscribeBuilder SetStartMessageId(MessageIdData startMessageId)
        {
            if(startMessageId != null)
            {
                _subscribe.StartMessageId = startMessageId;
            }
            return new CommandSubscribeBuilder(_subscribe);
        }
        public CommandSubscribeBuilder SetReadCompacted(long startMessageRollbackDurationInSec)
        {
            if (startMessageRollbackDurationInSec > 0)
            {
                _subscribe.StartMessageRollbackDurationSec = (ulong)startMessageRollbackDurationInSec;
            }
            return new CommandSubscribeBuilder(_subscribe);
        }
        public CommandSubscribeBuilder SetSubscription(string subscription)
        {
            _subscribe.Subscription = subscription;
            return new CommandSubscribeBuilder(_subscribe);
        }
        public CommandSubscribeBuilder SetSubType(SubType subType)
        {
            _subscribe.subType = subType;
            return new CommandSubscribeBuilder(_subscribe);
        }
        
        public CommandSubscribeBuilder SetTopic(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new NullReferenceException("Topic can not be empty");
            _subscribe.Topic = topic;
            return new CommandSubscribeBuilder(_subscribe);
        }
        public CommandSubscribe Build()
        {
            return _subscribe;
        }
        private Schema GetSchema(SchemaInfo schemaInfo)
        {
            return new SchemaBuilder()
                .SetName(schemaInfo)
                .SetType(schemaInfo)
                .SetSchemaData(schemaInfo).Build();
        }
    }
}
