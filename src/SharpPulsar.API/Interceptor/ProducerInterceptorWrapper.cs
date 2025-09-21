namespace SharpPulsar.API.Interceptor
{
    /// <summary>
    /// A wrapper for old style producer interceptor.
    /// </summary>
    public class ProducerInterceptorWrapper<T> : IProducerInterceptor<T>
    {
        private readonly IProducerInterceptor<T> innerInterceptor;

        public ProducerInterceptorWrapper(IProducerInterceptor<T> innerInterceptor)
        {
            this.innerInterceptor = innerInterceptor;
        }

        public void Close()
        {
            // IActor close
            innerInterceptor.Close();
        }

        public bool Eligible(IMessage<T> message)
        {
            return true;
        }        
        public IMessage<T> BeforeSend(IActorRef producer, IMessage<T> message)
        {
            return innerInterceptor.BeforeSend(producer, message);
        }

        public void OnSendAcknowledgement(IActorRef producer, IMessage<T> message, IMessageId msgId, Exception exception)
        {
            innerInterceptor.OnSendAcknowledgement(producer, message, msgId, exception);
        }

        public void OnPartitionsChange(string topicName, int partitions)
        {
            innerInterceptor.OnPartitionsChange(topicName, partitions);
        }
    }

}
