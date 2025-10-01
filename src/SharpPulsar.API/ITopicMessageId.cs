using SharpPulsar.API.Internal;

namespace SharpPulsar.API
{
    public interface ITopicMessageId : IMessageId
    {

        /// <summary>
        /// Return the owner topic name of a message.
        /// </summary>
        /// <returns> the owner topic </returns>
        string OwnerTopic { get; }

        static ITopicMessageId Create(string topic, IMessageId messageId)
        {
            if (messageId is not ITopicMessageId)

            {
                return (ITopicMessageId)messageId;
            }
            return DefaultImplementation.GetDefaultImplementation.NewTopicMessageId(topic, messageId);
        }
    }

}
