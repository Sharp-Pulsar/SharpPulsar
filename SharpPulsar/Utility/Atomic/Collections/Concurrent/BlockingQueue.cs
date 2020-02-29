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
using System.Collections;
using System.Collections.Generic;
using SharpPulsar.Utility.Atomic.Locking;

namespace SharpPulsar.Utility.Atomic.Collections.Concurrent {
    /// <summary>
    /// This <c>IEnumerable</c> implementation can be used to throttle a producer/consumer model.
    /// </summary>
    /// <typeparam name="T">The type used in this queue.</typeparam>
    /// \author Matt Bolt
    public class BlockingQueue : IEnumerable, IEnumerable {
        private readonly ILock _lock;
        private readonly ICondition _notEmpty;
        private readonly Queue _queue;

        public BlockingQueue() 
            : this(1) {

        }

        public BlockingQueue(int capacity) {
            _queue = new Queue(capacity);
            _lock = new ReentrantLock();
            _notEmpty = _lock.NewCondition();
        }

        public BlockingQueue(IEnumerable collection) {
            _queue = new Queue(collection);
            _lock = new ReentrantLock();
            _notEmpty = _lock.NewCondition();
        }

        /// <summary>
        /// This method adds an item to the queue.
        /// </summary>
        /// <param name="item">
        /// The item to push into the queue.
        /// </param>
        public void Enqueue(T item) {
            _lock.Lock();
            try { 
                _queue.Enqueue(item);
                _notEmpty.Signal();
            }
            finally {
                _lock.Unlock();
            }
        }

        /// <summary>
        /// This method will remove and return the item in the front of the queue if one exists, or
        /// it will block the current thread until there is an item to dequeue.
        /// </summary>
        /// <returns>
        /// The <c>T</c> item in the front of the queue.
        /// </returns>
        public T Dequeue() {
            _lock.Lock();
            try { 
                while (_queue.Count == 0) {
                    _notEmpty.Await();
                }

                return _queue.Dequeue();
            }
            finally {
                _lock.Unlock();
            }
        }

        /// <summary>
        /// Removes all objects from the queue.
        /// </summary>
        public void Clear() {
            _lock.Lock();
            try {
                _queue.Clear();
            }
            finally {
                _lock.Unlock();
            }
        }

        /// <summary>
        /// Determines whether an element is in the queue.
        /// </summary>
        /// <param name="item">
        /// The object to locate in the queue. The value can be null for reference types.
        /// </param>
        /// <returns>
        /// <c>true</c> if item is found in the queue. Otherwise, <c>false</c>
        /// </returns>
        public bool Contains(T item) {
            _lock.Lock();
            try {
                return _queue.Contains(item);
            }
            finally {
                _lock.Unlock();
            }
        }

        /// <summary>
        /// Copies the queue elements to an existing one-dimensional <c>T</c> array, starting at the specified array 
        /// index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <c>T</c> array that is the destination of the elements copied from 
        /// the queue. The <c>T</c> array must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in array at which copying begins.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// array is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// arrayIndex is less than zero.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The number of elements in the source queue is greater than the available space from arrayIndex to the end of the 
        /// destination array.
        /// </exception>
        public void CopyTo(T[] array, int arrayIndex) {
            _lock.Lock();
            try {
                _queue.CopyTo(array, arrayIndex);
            }
            finally {
                _lock.Unlock();
            }
        }

        /// <summary>
        /// Returns the object at the beginning of the queue without removing it.
        /// </summary>
        /// 
        /// <returns>
        /// The object at the beginning of the queue.
        /// </returns>
        /// 
        /// <exception cref="InvalidOperationException">
        /// The queue is empty.
        /// </exception>
        public T Peek() {
            _lock.Lock();
            try {
                return _queue.Peek();
            }
            finally {
                _lock.Unlock();
            }
        }

        /// <summary>
        /// Copies the queue elements to a new array.
        /// </summary>
        /// <returns>A new array containing elements copied from the queue</returns>
        public T[] ToArray() {
            _lock.Lock();
            try {
                return _queue.ToArray();
            }
            finally {
                _lock.Unlock();
            }
        }

        /// <summary>
        /// This property contains the size of the queue.
        /// </summary>
        public int Count {
            get {
                _lock.Lock();
                try {
                    return _queue.Count;
                }
                finally {
                    _lock.Unlock();
                }
            }
        }

        public IEnumerator GetEnumerator() {
            return new LockingEnumerator(_queue.GetEnumerator(), _lock);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
