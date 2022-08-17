using System;
using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;

namespace Sourcerer.Observer
{
    
    public class SourceObservable<T> : IObservable<T>
    {
        private readonly Source<T, NotUsed> _source;
        private readonly IMaterializer _materializer;

        public SourceObservable(Source<T, NotUsed> source, IMaterializer materializer)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _materializer = materializer ?? throw new ArgumentNullException(nameof(materializer));
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var subscriber = new ObserverSubscriber<T>(observer);
            var none = _source
                .ToMaterialized(
                    Sink.FromSubscriber<T>(subscriber),
                    Keep.None)
                .Run(_materializer);

            return subscriber;
        }
    }
}
