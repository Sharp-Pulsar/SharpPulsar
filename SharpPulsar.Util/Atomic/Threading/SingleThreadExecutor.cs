////////////////////////////////////////////////////////////////////////////////
//
//  MATTBOLT.BLOGSPOT.COM
//  Copyright(C) 2013 Matt Bolt
//
//  Permission is hereby granted, free of charge, to any person obtaining a 
//  copy of this software and associated documentation files (the "Software"), 
//  to deal in the Software without restriction, including without limitation 
//  the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//  and/or sell copies of the Software, and to permit persons to whom the 
//  Software is furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included 
//  in all copies or substantial portions of the Software
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
//  THE SOFTWARE.
//
////////////////////////////////////////////////////////////////////////////////

namespace SharpPulsar.Util.Atomic.Threading 
{

    using System;
    using System.Threading;
    using CSharp.Collections.Concurrent;


    /// <summary>
    /// This delegate is used to handle exceptions that occur in the executor. 
    /// </summary>
    /// <param name="exception">The <c>Exception</c> that occurred.</param>
    public delegate void ExceptionHandler(Exception exception);

    /// <summary>
    /// This executor uses a single thread and executes queued actions sequentially using an
    /// internal <c>BlockingQueue</c>. 
    /// </summary>
    /// \author Matt Bolt
    public class SingleThreadExecutor {

        private readonly BlockingQueue<Action> _actions;

        private Thread _thread;
        private ThreadPriority _priority;
        private AtomicBoolean _running;
        private AtomicBoolean _shuttingDown;

        /// <summary>
        /// Creates a new <c>SingleThreadExecutor</c> using a normal thread priority.
        /// </summary>
        public SingleThreadExecutor()
            : this(ThreadPriority.Normal) {
            
        }

        /// <summary>
        /// Creates a new <c>SingleThreadExecutor</c> using a custom thread priority.
        /// </summary>
        /// <param name="priority">
        /// The priority to assign the thread.
        /// </param>
        public SingleThreadExecutor(ThreadPriority priority) {
            _actions = new BlockingQueue<Action>();
            _running = new AtomicBoolean(false);
            _shuttingDown = new AtomicBoolean(false);

            _priority = priority;
        }

        /// <summary>
        /// Queues an <c>Action</c> for execution by a single thread. <c>Action</c> items are
        /// executed in the same order in which they are added.
        /// </summary>
        /// <param name="action">
        /// The <c>Action</c> delegate to queue for execution.
        /// </param>
        public void Execute(Action action) {
            if (_shuttingDown.Get()) {
                throw new Exception("Executor is shutting down.");
            }

            if (_running.CompareAndSet(false, true)) {
                _thread = new Thread(ExecuteAction);
                _thread.IsBackground = true;
                _thread.Priority = _priority;
                _thread.Start();
            }

            _actions.Enqueue(action);
        }

        /// <summary>
        /// Completes the current queue and joins the thread.
        /// </summary>
        public void Shutdown() {
            if (!_running.Get()) {
                return;
            }

            if (_shuttingDown.CompareAndSet(false, true)) {
                _actions.Enqueue(() => {
                    _running.CompareAndSet(true, false);
                });
                _thread.Join();
            }
        }

        /// <summary>
        /// Attempts to abort the <c>Thread</c> in the current state.
        /// </summary>
        public void ShutdownNow() {
            if (!_running.Get()) {
                return;
            }

            if (_shuttingDown.CompareAndSet(false, true)) {
                _running.CompareAndSet(true, false);

                try {
                    _thread.Abort();
                } catch (Exception e) {
                    if (null != OnException) {
                        OnException(e);
                    }
                }
            }
        }

        private void ExecuteAction() {
            while (_running.Get()) {
                try {
                    Action action = _actions.Dequeue();
                    action();
                } catch (Exception e) {
                    if (null != OnException) {
                        OnException(e);
                    }
                }
            }
        }

        /// <summary>
        /// The pending <c>Action</c>s left to execute.
        /// </summary>
        public int Pending {
            get {
                return _actions.Count;
            }
        }

        /// <summary>
        /// This event dispatches when an <c>Exception</c> occurs one of the queued <c>Action</c>
        /// executions.
        /// </summary>
        public event ExceptionHandler OnException;

    }
}
