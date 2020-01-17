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
/// 
namespace SharpPulsar.Util.Atomic.Locking 
{

    using System;
    using System.Threading;

    /// <summary>
    /// This class creates an inner object monitor, and exposes select methods from the <c>Monitor</c>
    /// class which use our inner monitor object as the parameter.
    /// </summary>
    public class LockMonitor {
        private readonly object @lock;

        public LockMonitor() {
            @lock = new object();
        }

        /// <summary>
        /// Use Enter to acquire the Monitor on the object passed as the parameter. If another thread 
        /// has executed an Enter, but has not yet executed the corresponding Exit, the current thread 
        /// will block until the other thread releases the object. It is legal for the same thread to 
        /// invoke Enter more than once without it blocking; however, an equal number of Exit calls must 
        /// be invoked before other threads waiting on the object will unblock.
        /// </summary>
        public void Enter() {
            Monitor.Enter(@lock);
        }

        public bool TryEnter() {
            return Monitor.TryEnter(@lock);
        }

        public bool TryEnter(int timeoutMs) {
            return Monitor.TryEnter(@lock, timeoutMs);
        }

        public bool TryEnter(TimeSpan timeout) {
            return Monitor.TryEnter(@lock, timeout);
        }

        /// <summary>
        /// The calling thread must own the lock before Exit. If the calling thread owns the lock, and has made 
        /// an equal number of Exit and Enter calls, then the lock is released. If the calling thread has not 
        /// invoked Exit as many times as Enter, the lock is not released.
        /// </summary>
        public void Exit() {
            Monitor.Exit(@lock);
        }

        /// <summary>
        /// Releases the lock on an object and blocks the current thread until it reacquires the lock.
        /// </summary>
        public bool Wait() {
            return Monitor.Wait(@lock);
        }

        public bool Wait(int timeoutMs) {
            return Monitor.Wait(@lock, timeoutMs);
        }

        public bool Wait(TimeSpan timeout) {
            return Monitor.Wait(@lock, timeout);
        }

        public void Pulse() {
            Monitor.Pulse(@lock);
        }

        public void PulseAll() {
            Monitor.PulseAll(@lock);
        }
    }
}

