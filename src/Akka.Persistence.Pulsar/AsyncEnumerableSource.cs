#region copyright
// -----------------------------------------------------------------------
//  <copyright file="AsyncEnumerableSource.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Akka.Streams;
using Akka.Streams.Stage;

namespace Akka.Persistence.Pulsar
{
    public sealed class AsyncEnumerableSourceStage<T> : GraphStage<SourceShape<T>>
    {
        #region logic

        private sealed class Logic : OutGraphStageLogic
        {
            private readonly IAsyncEnumerator<T> enumerator;
            private readonly Outlet<T> outlet;
            private readonly Action<T> onSuccess;
            private readonly Action<Exception> onFailure;
            private readonly Action onComplete;
            private readonly Action<Task<bool>> handleContinuation;

            public Logic(SourceShape<T> shape, IAsyncEnumerator<T> enumerator) : base(shape)
            {
                this.enumerator = enumerator;
                this.outlet = shape.Outlet;
                this.onSuccess = GetAsyncCallback<T>(this.OnSuccess);
                this.onFailure = GetAsyncCallback<Exception>(this.OnFailure);
                this.onComplete = GetAsyncCallback(this.OnComplete);
                this.handleContinuation = task =>
                {
                    // Since this Action is used as task continuation, we cannot safely call corresponding
                    // OnSuccess/OnFailure/OnComplete methods directly. We need to do that via async callbacks.
                    if (task.IsFaulted) this.onFailure(task.Exception);
                    else if (task.IsCanceled) this.onFailure(new TaskCanceledException(task));
                    else if (task.Result) this.onSuccess(enumerator.Current);
                    else this.onComplete();
                };
                
                this.SetHandler(this.outlet, this);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void OnComplete() => this.CompleteStage();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void OnFailure(Exception exception) => FailStage(exception);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void OnSuccess(T element) => Push(this.outlet, element);

            public override void OnPull()
            {
                var vtask = enumerator.MoveNextAsync();
                if (vtask.IsCompletedSuccessfully)
                {
                    // When MoveNextAsync returned immediatelly, we don't need to await.
                    // We can use fast path instead.
                    if (vtask.Result)
                    {
                        // if result is true, it means we got an element. Push it downstream.
                        Push(this.outlet, enumerator.Current);
                    }
                    else
                    {
                        // if result is false, it means enumerator was closed. Complete stage in that case.
                        CompleteStage();
                    }
                }
                else
                {
                    vtask.AsTask().ContinueWith(this.handleContinuation);
                }
            }

            public override void OnDownstreamFinish()
            {
                var vtask = this.enumerator.DisposeAsync();
                if (vtask.IsCompletedSuccessfully)
                {
                    this.CompleteStage(); // if dispose completed immediately, complete stage directly
                }
                else
                {
                    // for async disposals use async callback
                    vtask.GetAwaiter().OnCompleted(this.onComplete);
                }
            }
        }

        #endregion
        
        private readonly Outlet<T> outlet = new Outlet<T>("asyncEnumerable.out");
        private readonly IAsyncEnumerable<T> asyncEnumerable;

        public AsyncEnumerableSourceStage(IAsyncEnumerable<T> asyncEnumerable)
        {
            //TODO: when to dispose async enumerable? Should this be a part of ownership of current stage, or should it
            // be a responsibility of the caller?
            this.asyncEnumerable = asyncEnumerable;
            Shape = new SourceShape<T>(outlet);
        }

        public override SourceShape<T> Shape { get; } 
        protected override GraphStageLogic CreateLogic(Attributes inheritedAttributes) => new Logic(this.Shape, this.asyncEnumerable.GetAsyncEnumerator());
    }
}