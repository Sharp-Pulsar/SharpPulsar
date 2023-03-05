
namespace SharpPulsar.Protocol.Proto
{
    public static class BaseCommandExtension
    {
        public static BaseCommand ToBaseCommand(this CommandConnect connect)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Connect,
                Connect = connect
            };
        }
        public static BaseCommand ToBaseCommand(this CommandAck ack)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Ack,
                Ack = ack
            };
        }
        public static BaseCommand ToBaseCommand(this CommandActiveConsumerChange value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.ActiveConsumerChange,
                ActiveConsumerChange = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandAddPartitionToTxn value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.AddPartitionToTxn,
                addPartitionToTxn = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandAddSubscriptionToTxn value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.AddSubscriptionToTxn,
                addSubscriptionToTxn = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandAuthChallenge value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.AuthChallenge,
                authChallenge = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandAuthResponse value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.AuthResponse,
                authResponse = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandCloseConsumer value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.CloseConsumer,
                CloseConsumer = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandCloseProducer value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.CloseProducer,
                CloseProducer = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandConsumerStats value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.ConsumerStats,
                consumerStats = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandEndTxn value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.EndTxn,
                endTxn = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandEndTxnOnPartition value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.EndTxnOnPartition,
                endTxnOnPartition = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandEndTxnOnSubscription value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.EndTxnOnSubscription,
                endTxnOnSubscription = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandError value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Error,
                Error = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandFlow value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Flow,
                Flow = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandGetLastMessageId value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.GetLastMessageId,
                getLastMessageId = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandGetOrCreateSchema value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.GetOrCreateSchema,
                getOrCreateSchema = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandGetSchema value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.GetSchema,
                getSchema = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandGetTopicsOfNamespace value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.GetTopicsOfNamespace,
                getTopicsOfNamespace = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandLookupTopic value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Lookup,
                lookupTopic = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandNewTxn value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.NewTxn,
                newTxn = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandPartitionedTopicMetadata value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.PartitionedMetadata,
                partitionMetadata = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandPing value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Ping,
                Ping = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandPong value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Pong,
                Pong = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandProducer value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Producer,
                Producer = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandReachedEndOfTopic value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.ReachedEndOfTopic,
                reachedEndOfTopic = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandRedeliverUnacknowledgedMessages value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.RedeliverUnacknowledgedMessages,
                redeliverUnacknowledgedMessages = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandSeek value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Seek,
                Seek = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandSend value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Send,
                Send = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandSendError value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.SendError,
                SendError = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandSubscribe value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Subscribe,
                Subscribe = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandUnsubscribe value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Unsubscribe,
                Unsubscribe = value
            };
        } 
        public static BaseCommand ToBaseCommand(this CommandTcClientConnectRequest value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.TcClientConnectRequest,
                tcClientConnectRequest = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandWatchTopicListClose value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.WatchTopicListClose,
                watchTopicListClose = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandWatchTopicList value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.WatchTopicList,
                watchTopicList = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandWatchTopicListSuccess value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.WatchTopicListSuccess,
                watchTopicListSuccess = value
            };
        }
        public static BaseCommand ToBaseCommand(this CommandWatchTopicUpdate value)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.WatchTopicUpdate,
                watchTopicUpdate = value
            };
        }
    }
}
