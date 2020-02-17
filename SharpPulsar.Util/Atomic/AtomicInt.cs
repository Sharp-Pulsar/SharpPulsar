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

using System.Threading;

namespace SharpPulsar.Utility.Atomic
{
    /// <summary>
    /// Provides lock-free atomic read/write utility for a <c>int</c> value. The atomic classes found in this package
    /// were are meant to replicate the <c>java.util.concurrent.atomic</c> package in Java by Doug Lea. The two main differences
    /// are implicit casting back to the <c>int</c> data type, and the use of a non-volatile inner variable.
    /// 
    /// <para>The internals of these classes contain wrapped usage of the <c>System.Threading.Interlocked</c> class, which is how
    /// we are able to provide atomic operation without the use of locks. </para>
    /// </summary>
    /// <remarks>
    /// It's also important to note that <c>++</c> and <c>--</c> are never atomic, and one of the main reasons this class is 
    /// needed. I don't believe its possible to overload these operators in a way that is autonomous.
    /// </remarks>
    /// \author Matt Bolt
    public class AtomicInt {

        private int _value;

        /// <summary>
        /// Creates a new <c>AtomicInt</c> instance with an initial value of <c>0</c>.
        /// </summary>
        public AtomicInt()
            : this(0) {

        }

        /// <summary>
        /// Creates a new <c>AtomicInt</c> instance with the initial value provided.
        /// </summary>
        public AtomicInt(int value) {
            _value = value;
        }

        /// <summary>
        /// This method returns the current value.
        /// </summary>
        /// <returns>
        /// The value of the <c>int</c> accessed atomically.
        /// </returns>
        public int Get() {
            return _value;
        }

        /// <summary>
        /// This method sets the current value atomically.
        /// </summary>
        /// <param name="value">
        /// The new value to set.
        /// </param>
        public void Set(int value) {
            Interlocked.Exchange(ref _value, value);
        }

        /// <summary>
        /// This method atomically sets the value and returns the original value.
        /// </summary>
        /// <param name="value">
        /// The new value.
        /// </param>
        /// <returns>
        /// The value before setting to the new value.
        /// </returns>
        public int GetAndSet(int value) {
            return Interlocked.Exchange(ref _value, value);
        }

        /// <summary>
        /// Atomically sets the value to the given updated value if the current value <c>==</c> the expected value.
        /// </summary>
        /// <param name="expected">
        /// The value to compare against.
        /// </param>
        /// <param name="result">
        /// The value to set if the value is equal to the <c>expected</c> value.
        /// </param>
        /// <returns>
        /// <c>true</c> if the comparison and set was successful. A <c>false</c> indicates the comparison failed.
        /// </returns>
        public bool CompareAndSet(int expected, int result) {
            return Interlocked.CompareExchange(ref _value, result, expected) == expected;
        }

        /// <summary>
        /// Atomically adds the given value to the current value.
        /// </summary>
        /// <param name="delta">
        /// The value to add.
        /// </param>
        /// <returns>
        /// The updated value.
        /// </returns>
        public int AddAndGet(int delta) {
            return Interlocked.Add(ref _value, delta);
        }

        /// <summary>
        /// This method atomically adds a <c>delta</c> the value and returns the original value.
        /// </summary>
        /// <param name="delta">
        /// The value to add to the existing value.
        /// </param>
        /// <returns>
        /// The value before adding the delta.
        /// </returns>
        public int GetAndAdd(int delta) {
            for (;;) {
                int current = Get();
                int next = current + delta;
                if (CompareAndSet(current, next)) {
                    return current;
                }
            }
        }

        /// <summary>
        /// This method increments the value by 1 and returns the previous value. This is the atomic 
        /// version of post-increment.
        /// </summary>
        /// <returns>
        /// The value before incrementing.
        /// </returns>
        public int Increment() {
            return GetAndAdd(1);
        }

        /// <summary>
        /// This method decrements the value by 1 and returns the previous value. This is the atomic 
        /// version of post-decrement.
        /// </summary>
        /// <returns>
        /// The value before decrementing.
        /// </returns>
        public int Decrement() {
            return GetAndAdd(-1);
        }

        /// <summary>
        /// This method increments the value by 1 and returns the new value. This is the atomic version 
        /// of pre-increment.
        /// </summary>
        /// <returns>
        /// The value after incrementing.
        /// </returns>
        public int PreIncrement() {
            return Interlocked.Increment(ref _value);
        }

        /// <summary>
        /// This method decrements the value by 1 and returns the new value. This is the atomic version 
        /// of pre-decrement.
        /// </summary>
        /// <returns>
        /// The value after decrementing.
        /// </returns>
        public int PreDecrement() {
            return Interlocked.Decrement(ref _value);
        }

        /// <summary>
        /// This operator allows an implicit cast from <c>AtomicInt</c> to <c>int</c>.
        /// </summary>
        public static implicit operator int(AtomicInt value) {
            return value.Get();
        }

    }

}