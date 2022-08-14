using System;
using System.Threading.Tasks;
using Reactive.Streams;

namespace Sourcerer.Observer
{
    // TODO Double check the reactive streams spec.
    public class ObserverSubscriber<T> : ISubscriber<T>, IDisposable
    {
        // Is this a good idea?
        public Task OnSubscribeTask { get; }

        private readonly IObserver<T> _observer;
        private readonly TaskCompletionSource<object> _taskCompletionSource;
        private ISubscription _subscription;

        public ObserverSubscriber(IObserver<T> observer)
        {
            _observer = observer ?? throw new ArgumentNullException(nameof(observer));
            _taskCompletionSource = new TaskCompletionSource<object>();

            OnSubscribeTask = _taskCompletionSource.Task;
        }

        public void OnSubscribe(ISubscription subscription)
        {
            if (_subscription != null)
            {
                throw new InvalidOperationException("Multiple calls to OnSubscribe() are not allowed.");
            }

            _subscription = subscription;
            _subscription.Request(1);
            _taskCompletionSource.SetResult(new object());
        }

        public void OnComplete()
        {
            if (_subscription == null)
            {
                throw new InvalidOperationException("Cannot call OnComplete before OnSubscribe.");
            }

            _observer.OnCompleted();
            _subscription.Cancel();
        }

        public void OnError(Exception cause)
        {
            if (_subscription == null)
            {
                throw new InvalidOperationException("Cannot call OnError before OnSubscribe.");
            }

            _observer.OnError(cause);
            _subscription.Cancel();
        }

        public void OnNext(T element)
        {
            if (_subscription == null)
            {
                throw new InvalidOperationException("OnSubscribe must be called before OnNext.");
            }

            _observer.OnNext(element);
            _subscription.Request(1);
        }

        public void Dispose()
        {
            _subscription?.Cancel();
        }
    }
}
