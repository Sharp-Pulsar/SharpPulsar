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

using System.Collections.Generic;
using SharpPulsar.Utility.Atomic.Collections;
using SharpPulsar.Utility.Atomic.Util;

namespace SharpPulsar.Utility.Atomic.Locking 
{
    /// <summary>
    /// The default <c>ICondition</c> implementation for use with <c>CSharp.Locking.ReentrantLock</c>. 
    /// It allows specific conditions to wait and signal, while being intrinsic to the root lock.
    /// </summary>
    public class LockCondition : ICondition {

        private readonly string _signalId;
        private readonly LockMonitor _monitor;
        private readonly HashSet<string> _waitingThreads;
        private readonly HashStack<string, string> _threadSignals;

        /// <summary>
        /// Initializes a new instance of the <see cref="LockCondition"/> class.
        /// </summary>
        /// <param name="signalId">Signal identifier used to notify waiting threads which condition was statisfied.</param>
        /// <param name="monitor">The <c>LockMonitor</c> used in the parent lock.</param>
        public LockCondition(string signalId, LockMonitor monitor) {
            _signalId = signalId;
            _monitor = monitor;
            _waitingThreads = new HashSet<string>();
            _threadSignals = new HashStack<string, string>();
        }

        /// <summary>
        /// Signals all threads waiting on this condition.
        /// </summary>
        public void Signal() {
            // For signal, we iterate through all of the waiting threads and push the condition
            // id onto the stack for that particular thread. Once this is done, we are safe to 
            // pulse the monitor to release waiting threads. 
            foreach (string threadId in _waitingThreads) {
                _threadSignals.Push(threadId, _signalId);
            }

            _monitor.PulseAll();
        }

        /// <summary>
        /// Blocks the current thread until the condition is signaled.
        /// </summary>
        public void Await() {
            _waitingThreads.Add(ThreadHelper.CurrentThreadId);

            // Wait until signaled, at which time, we check to see if the signal
            // for the released thread matches this condition's id, and if so, the
            // thread is no longer in the waiting state
            string signal = null;
            while (signal != _signalId) {
                _monitor.Wait();

                signal = _threadSignals.Pop(ThreadHelper.CurrentThreadId);
            }

            // If we've made it this far, the current thread is no longer waiting
            _waitingThreads.Remove(ThreadHelper.CurrentThreadId);
        }

    }
}

