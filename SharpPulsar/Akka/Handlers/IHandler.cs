using SharpPulsar.Impl;

namespace SharpPulsar.Akka.Handlers
{
    public interface IHandler
    {
        /// <summary>
        /// Due to the nature of Actor Model which is async, the purpose of this interface is to help end users handle responses from the system
        /// </summary>
        /// <param name="data"></param>
        void Capture(object data);
        /// <summary>
        /// Captures Pulsar  MessageId Info or Status
        /// </summary>
        /// <param name="messageid"></param>
        void MessageId(object messageid);
    }
}
