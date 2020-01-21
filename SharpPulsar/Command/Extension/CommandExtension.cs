using SharpPulsar.Common.PulsarApi;
using static SharpPulsar.Exception.PulsarClientException;

namespace SharpPulsar.Command.Extension
{
    public static class CommandExtension
    {
        public static void Throw(this CommandSendError command) => Throw(command.Error, command.Message);

        public static void Throw(this CommandLookupTopicResponse command) => Throw(command.Error, command.Message);

        public static void Throw(this CommandError error) => Throw(error.Error, error.Message);

        private static void Throw(ServerError error, string message)
        {
            switch (error)
            {
                case ServerError.AuthenticationError: throw new AuthenticationException(message);
                case ServerError.AuthorizationError: throw new AuthorizationException(message);
                case ServerError.ChecksumError: throw new ChecksumException(message);
                case ServerError.ConsumerAssignError: throw new ConsumerAssignException(message);
                case ServerError.ConsumerBusy: throw new ConsumerBusyException(message);
                case ServerError.ConsumerNotFound: throw new ConsumerNotFoundException(message);
                case ServerError.IncompatibleSchema: throw new IncompatibleSchemaException(message);
                case ServerError.InvalidTopicName: throw new InvalidTopicNameException(message);
                case ServerError.MetadataError: throw new MetadataException(message);
                case ServerError.PersistenceError: throw new PersistenceException(message);
                case ServerError.ProducerBlockedQuotaExceededError:
                case ServerError.ProducerBlockedQuotaExceededException:
                    throw new ProducerBlockedQuotaExceededException(message + ". Error code: " + error);
                case ServerError.ProducerBusy: throw new ProducerBusyException(message);
                case ServerError.ServiceNotReady: throw new ServiceNotReadyException(message);
                case ServerError.SubscriptionNotFound: throw new SubscriptionNotFoundException(message);
                case ServerError.TooManyRequests: throw new TooManyRequestsException(message);
                case ServerError.TopicNotFound: throw new TopicNotFoundException(message);
                case ServerError.TopicTerminatedError: throw new TopicTerminatedException(message);
                case ServerError.UnsupportedVersionError: throw new UnsupportedVersionException(message);
                case ServerError.UnknownError:
                default: throw new UnknownException(message + ". Error code: " + error);
            }
        }
        public static BaseCommand ToBaseCommand(this CommandAck command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Ack,
                Ack = command
            };
        }

        public static BaseCommand ToBaseCommand(this CommandActiveConsumerChange command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.ActiveConsumerChange,
                ActiveConsumerChange = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandAddPartitionToTxn command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.AddPartitionToTxn,
                addPartitionToTxn = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandAddSubscriptionToTxn command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.AddSubscriptionToTxn,
                addSubscriptionToTxn = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandAuthChallenge command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.AuthChallenge,
                authChallenge = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandCloseConsumer command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.CloseConsumer,
                CloseConsumer = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandCloseProducer command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.CloseProducer,
                CloseProducer = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandConnect command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Connect,
                Connect = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandConsumerStats command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.ConsumerStats,
                consumerStats = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandEndTxn command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.EndTxn,
                endTxn = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandEndTxnOnPartition command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.EndTxnOnPartition,
                endTxnOnPartition = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandEndTxnOnSubscription command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.EndTxnOnSubscription,
                endTxnOnSubscription = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandError command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Error,
                Error = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandFlow command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Flow,
                Flow = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandGetLastMessageId command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.GetLastMessageId,
                getLastMessageId = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandGetOrCreateSchema command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.GetOrCreateSchema,
                getOrCreateSchema = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandGetSchema command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.GetSchema,
                getSchema = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandGetTopicsOfNamespace command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.GetTopicsOfNamespace,
                getTopicsOfNamespace = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandLookupTopic command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Lookup,
                lookupTopic = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandMessage command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Message,
                Message = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandNewTxn command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.NewTxn,
                newTxn = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandPartitionedTopicMetadata command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.PartitionedMetadata,
                partitionMetadata = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandProducer command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Producer,
                Producer = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandProducerSuccess command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.ProducerSuccess,
                ProducerSuccess = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandReachedEndOfTopic command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.ReachedEndOfTopic,
                reachedEndOfTopic = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandRedeliverUnacknowledgedMessages command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.RedeliverUnacknowledgedMessages,
                redeliverUnacknowledgedMessages = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandSeek command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Seek,
                Seek = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandSend command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Send,
                Send = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandSendError command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.SendError,
                SendError = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandSendReceipt command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.SendReceipt,
                SendReceipt = command
            };            
        }
        public static BaseCommand ToBaseCommand(this CommandSubscribe command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Subscribe,
                Subscribe = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandSuccess command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Success,
                Success = command
            };
        }
        public static BaseCommand ToBaseCommand(this CommandUnsubscribe command)
        {
            return new BaseCommand
            {
                type = BaseCommand.Type.Unsubscribe,
                Unsubscribe = command
            };
        }
    }
}
