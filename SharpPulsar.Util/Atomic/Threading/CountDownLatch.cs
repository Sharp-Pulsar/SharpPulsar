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


    /// <summary>
    /// This class acts as a lock-less, thread safe count down which can be used to perform multiple async tasks 
    /// and await signals from all threads without having to wait for the thread to terminate. It's based on the 
    /// <see href="http://docs.oracle.com/javase/7/docs/api/java/util/concurrent/CountDownLatch.html">
    /// java.util.concurrent.CountDownLatch</see> class found in Java.
    /// </summary>
    /// <remarks>
    /// This class performs a very similar task to the <c>CountDownEvent</c> class from .NET 4.5, but is compatible 
    /// with .NET 3.0+ for use with platforms like Unity3D.
    /// </remarks>
    /// \author Matt Bolt
    public class CountDownLatch {

        private AtomicInt _count;
        private ManualResetEvent _wait;

        /// <summary>
        /// Creates a new <c>CountDownLatch</c> instance with the provided counter initializer.
        /// </summary>
        /// <param name="count">
        /// The initial value of the counter. Note that this value must be greater than 0.
        /// </param>
        public CountDownLatch(int count) {
            if (count <= 0) {
                throw new Exception("Count value cannot be less than or equal to 0.");
            }

            _wait = new ManualResetEvent(false);
            _count = new AtomicInt(count);
        }

        /// <summary>
        /// This method decrements the counter by <c>1</c>. If the counter reaches 0, the reset event is set.
        /// </summary>
        public void CountDown() {
            if (_count.PreDecrement() <= 0) {
                _wait.Set();
            }
        }


        /// <summary>
        /// Blocks execution of the current thread until the counter has reached 0.
        /// </summary>
        public void Await() {
            _wait.WaitOne();
        }

        /// <summary>
        /// Blocks execution of the current thread until the counter has reached 0 or the timeout expires.
        /// </summary>
        /// <param name="millisecondsTimeout">
        /// The total amount of time, in milliseconds, to wait before continuing.
        /// </param>
        public void Await(int millisecondsTimeout) {
            _wait.WaitOne(millisecondsTimeout);
        }

        /// <summary>
        /// Blocks execution of the current thread until the counter has reached 0 or the timeout expires.
        /// </summary>
        /// <param name="timeout">
        /// The <c>TimeSpan</c> instance used as the time to wait before continuing.
        /// </param>
        public void Await(TimeSpan timeout) {
            _wait.WaitOne(timeout);
        }

        /// <summary>
        /// The current value of the counter.
        /// </summary>
        public int CurrentCount {
            get {
                return _count.Get();
            }
        }
    }
}