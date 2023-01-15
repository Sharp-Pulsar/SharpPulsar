
using Akka.Actor;

namespace SharpPulsar.Interfaces
{
    /// <summary>
    /// Reader interceptor. </summary>
    /// @param <T> </param>
    public interface IReaderInterceptor<T>
    {

        /// <summary>
        /// Close the interceptor.
        /// </summary>
        void Close();

        /// <summary>
        /// This is called just before the message is returned by
        /// <seealso cref="Reader.readNext()"/>, <seealso cref="ReaderListener.received(IReader, IMessage)"/>
        /// or the <seealso cref="java.util.concurrent.CompletableFuture"/> returned by
        /// <seealso cref="Reader.readNextAsync()"/> completes.
        /// 
        /// This method is based on <seealso cref="ConsumerInterceptor.beforeConsume(IConsumer, IMessage)"/>,
        /// so it has the same features.
        /// </summary>
        /// <param name="reader"> the reader which contains the interceptor </param>
        /// <param name="message"> the message to be read by the client. </param>
        /// <returns> message that is either modified by the interceptor or same message
        ///         passed into the method. </returns>
        IMessage<T> BeforeRead(IActorRef reader, IMessage<T> message);

        /// <summary>
        /// This method is called when partitions of the topic (partitioned-topic) changes.
        /// </summary>
        /// <param name="topicName"> topic name </param>
        /// <param name="partitions"> new updated number of partitions </param>
        void OnPartitionsChange(string topicName, int partitions)
        {

        }

    }

}
