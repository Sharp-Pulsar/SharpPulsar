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

using System;
using System.Threading;

namespace SharpPulsar.Utility.Atomic.Locking 
{
    /// <summary>
    /// A reentrant mutual exclusion <c>ILock</c> with the same basic behavior and semantics as the 
    /// implicit monitor lock accessed using the <c>lock</c> keyword, but with some additional capabilities.
    /// 
    /// <para>
    /// A <c>ReentrantLock</c> is owned by the thread last successfully locking, but not yet unlocking 
    /// it. A thread invoking lock will return, successfully acquiring the lock, when the lock is not 
    /// owned by another thread. The method will return immediately if the current thread already owns 
    /// the lock. This can be checked using the property <c>IsHeldByCurrentThread</c>
    /// </para>
    /// </summary>
    /// <remarks>
    /// This class does not currently support fairness like the java version of this lock,
    /// <c>java.util.concurrent.locks.ReentrantLock</c>
    /// </remarks>
    /// \author Matt Bolt
    public class ReentrantLock : ILock {
        private readonly LockMonitor _monitor;
        private readonly ConditionIds _ids;

        private Thread _lockHolder;
        private uint _lockCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReentrantLock"/> class.
        /// </summary>
        public ReentrantLock() {
            _ids = new ConditionIds("REL", 1);
            _monitor = new LockMonitor();
            _lockCount = 0;
        }

        /// <summary>
        /// Locks the the current instance, 
        /// </summary>
        public void Lock() {
            _monitor.Enter();

            _lockCount++;
            _lockHolder = Thread.CurrentThread;
        }

        /// <summary>
        /// Attempts to obtain the lock for this instance, at most, waiting the length of 
        /// the <c>timeoutMs</c> specified. 
        /// </summary>
        /// <param name="timeoutMs">The maximum time to wait before failing, in milliseconds.</param>
        /// <returns>
        /// <c>true</c> if the thread obtains the lock, or <c>false</c> if the timeout occurs
        /// before successfully obtaining the lock.
        /// </returns>
        public bool TryLock(int timeoutMs) {
            bool success = _monitor.TryEnter(timeoutMs);
            if (success) {
                _lockCount++;
                _lockHolder = Thread.CurrentThread;
            }
            return success;
        }

        /// <summary>
        /// Attempts to obtain the lock for this instance, at most, waiting the length of 
        /// the <c>timeout</c> specified. 
        /// </summary>
        /// <param name="timeout">The maximum time to wait before failing..</param>
        /// <returns>
        /// <c>true</c> if the thread obtains the lock, or <c>false</c> if the timeout occurs
        /// before successfully obtaining the lock.
        /// </returns>
        public bool TryLock(TimeSpan timeout) {
            bool success = _monitor.TryEnter(timeout);
            if (success) {
                _lockCount++;
                _lockHolder = Thread.CurrentThread;
            }
            return success;
        }

        /// <summary>
        /// Creates a new <c>LockCondition</c> implementation for use with the current lock.
        /// </summary>
        /// <returns>A new condition instance used for flow control within a lock.</returns>
        public ICondition NewCondition() {
            return new LockCondition(_ids.Next(), _monitor);
        }

        /// <summary>
        /// Releases the lock.
        /// </summary>
        public void Unlock() {
            if (--_lockCount <= 0) {
                _lockCount = 0;
                _lockHolder = null;
            }

            _monitor.Exit();
        }

        /// <summary>
        /// Whther or not the current thread is holding the lock or not.
        /// </summary>
        /// <value><c>true</c> if the current thread is holding the lock; otherwise, <c>false</c></value>
        public bool IsHeldByCurrentThread {
            get {
                return _lockHolder == Thread.CurrentThread;
            }
        }

    }
}

