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

namespace SharpPulsar.Util.Atomic.Collections 
{

    using System;
    using System.Collections.Generic;


    /// <summary>
    /// This collection implementation uses an internal <c>Dictionary</c> as a hash 
    /// table using <c>Stack</c> instances for buckets. 
    /// <para>
    /// The <c>Push</c> operation lazily creates a <c>Stack</c> for the instance if
    /// one does not already exist for the key
    /// </para>
    /// <para>
    /// Note that this class is not thread safe by default. This is not a requirement 
    /// for it's use in <c>LockCondition</c>, since each waiting thread releases one
    /// at a time.
    /// </para>
    /// </summary>
    /// \author Matt Bolt
    public class HashStack<T,U>  {
        private readonly Dictionary<T, Stack<U>> _hashStack;

        public HashStack() {
            _hashStack = new Dictionary<T, Stack<U>>();
        }

        /// <summary>
        /// Push the value onto the <c>Stack</c> using the specified key. Note that 
        /// this method will lazily create a <c>Stack</c> if one does not exist for 
        /// the key provided. 
        /// </summary>
        /// <param name="key">The key to use to represent the <c>Stack</c></param>
        /// <param name="value">The value to store in the <c>Stack</c>.</param>
        public void Push(T key, U value) {
            if (!_hashStack.ContainsKey(key)) {
                _hashStack.Add(key, new Stack<U>());
            }
            _hashStack[key].Push(value);
        }

        /// <summary>
        /// Pops the value on the <c>Stack</c> specified by the key and returns the value.
        /// If the <c>Stack</c> does not exist or the <c>Stack</c> is empty for that key,
        /// then the <c>default</c> value is returned.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>
        /// If the <c>Stack</c> does not exist or the <c>Stack</c> is empty for that key,
        /// then the <c>default</c> value is returned.
        /// </returns>
        public U Pop(T key) {
            if (!_hashStack.ContainsKey(key) || IsStackEmpty(key)) {
                return default(U);
            }

            return _hashStack[key].Pop();
        }

        /// <summary>
        /// This method removes an entire <c>Stack</c> from the table and returns it. If the
        /// <c>Stack</c> instance for the key doesn't exist, then <c>null</c> is returned.
        /// </summary>
        /// <param name="key">The key to use for looking up the <c>Stack</c> instance.</param>
        /// <returns>The <c>Stack</c> instance that was removed. If an instance didn't exist for
        /// the key, then <c>null</c> is returned.</returns>
        public Stack<U> Remove(T key) {
            Stack<U> stack = null;
            if (_hashStack.ContainsKey(key)) {
                stack = _hashStack[key];
                _hashStack.Remove(key);
            }
            return stack;
        }

        /// <summary>
        /// Whether or not a <c>Stack</c> instance exists for the key specified.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns><c>true</c>, if the <c>Stack</c> exists, <c>false</c> otherwise.</returns>
        public bool ContainsKey(T key) {
            return _hashStack.ContainsKey(key);
        }

        /// <summary>
        /// Whether or not a the <c>value</c> specified exists in the <c>Stack</c> under the key.
        /// </summary>
        /// <param name="key">The key used to look up the <c>Stack</c> instance.</param>
        /// <param name="value">The value to check for in the <c>Stack</c>.</param>
        /// <returns><c>true</c>, if the <c>Stack</c> exists and the value exists in the stack, <c>false</c> otherwise.</returns>
        public bool ContainsValue(T key, U value) {
            return _hashStack.ContainsKey(key) && _hashStack[key].Contains(value);
        }

        /// <summary>
        /// Looks up and returns the stack element count for the provided key.
        /// </summary>
        /// <param name="key">The key used to look up the stack instance.</param>
        /// <returns>The Count for the stack under the specified key.</returns>
        public int CountFor(T key) {
            if (!_hashStack.ContainsKey(key)) {
                return 0;
            }
            return _hashStack[key].Count;
        }

        /// <summary>
        /// Determines whether the stack under the specified key is empty.
        /// </summary>
        /// <param name="key">The key used to look up the stack instance</param>
        /// <returns><c>true</c> if the stack doesn't exist or contains 0 elements.</returns>
        public bool IsStackEmpty(T key) {
            return CountFor(key) == 0;
        }

        /// <summary>
        /// The total number of key entries in the table.
        /// </summary>
        /// <value>The total number of key entries exist in the table</value>
        public int Count {
            get {
                return _hashStack.Count;
            }
        }

        /// <summary>
        /// Whether or not the table has any key entries.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty {
            get {
                return _hashStack.Count == 0;
            }
        }

    }
}

