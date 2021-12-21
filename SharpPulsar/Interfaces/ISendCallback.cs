using System;
using System.Threading.Tasks;

namespace SharpPulsar.Interfaces
{
    public interface ISendCallback<T>
    {

        // / <summary>
        // / invoked when send operation completes
        // / </summary>
        // / <param name="e"> </param>
        void SendComplete(Exception e);

        // / <summary>
        // / used to specify a callback to be invoked on completion of a send operation for individual messages sent in a
        // / batch. Callbacks for messages in a batch get chained
        // / </summary>
        // / <param name="msg"> message sent </param>
        // / <param name="scb"> callback associated with the message </param>
        void AddCallback(Message<T> msg, ISendCallback<T> scb);

        // / 
        // / <returns> next callback in chain </returns>
        ISendCallback<T> NextSendCallback { get; }

        // / <summary>
        // / Return next message in chain
        // / </summary>
        // / <returns> next message in chain </returns>
        Message<T> NextMessage { get; }

        // / 
        // / <returns> future associated with callback </returns>
        TaskCompletionSource<Message<T>> Future { get; }
    }

}
