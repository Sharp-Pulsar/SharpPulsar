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
using System.Collections.Generic;

namespace SharpPulsar.Utility.Atomic.Threading 
{
    /// <summary>
    /// This class stores a static instance of <c>T</c> in each <c>Thread</c> it is
    /// accessed. It uses an internal implementation of the <c>ThreadStatic</c>
    /// attribute. 
    /// </summary>
    /// <typeparam name="T">The type local to each thread.</typeparam>
    /// \author Matt Bolt
    public class ThreadLocal<T> {

        // Generate a unique ThreadLocal identifier for each instance
        private static readonly AtomicInt ThreadLocalIds;

        [ThreadStatic]
        private static Dictionary<int, T> Instances;

        static ThreadLocal() {
            ThreadLocalIds = new AtomicInt(0);
        }

        private int _id;
        private Func<T> _factory;

        /// <summary>
        /// Creates a new <c>ThreadLocal</c> instance using a <c>Func</c> capable
        /// of creating new <c>T</c> instances.
        /// </summary>
        /// <param name="factory">
        /// The <c>Func</c> instance used to create <c>T</c> instances.
        /// </param>
        public ThreadLocal(Func<T> factory) {
            _id = ThreadLocalIds.Increment();
            _factory = factory;
        }

        /// <summary>
        /// This method retreives the <c>T</c> instance local to the current <c>Thread</c>. 
        /// If an instance does not exist, one is created via the <c>Func</c> passed
        /// via the constructor.
        /// </summary>
        /// <returns>
        /// The <c>T</c> instance local to the current thread.
        /// </returns>
        public T Get() {
            if (null == Instances) {
                Instances = new Dictionary<int, T>();
            }

            if (!Instances.ContainsKey(_id)) {
                Instances[_id] = _factory();
            }

            return Instances[_id];
        }

        /// <summary>
        /// This method allows you to set the <c>T</c> instance local to the current
        /// <c>Thread</c>. 
        /// </summary>
        /// <param name="instance">
        /// The <c>T</c> instance to set on the local <c>Thread</c>.
        /// </param>
        public void Set(T instance) {
            if (null == Instances) {
                Instances = new Dictionary<int, T>();
            }

            Instances[_id] = instance;
        }
    }
    
}
