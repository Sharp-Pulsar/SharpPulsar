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

namespace SharpPulsar.Util.Atomic.Locking 
{


    /// <summary>
    /// This class is used to generate condition ids for lock instances. The ids generated
    /// need only be unique per each lock instance, and are *not* exposed outside of the lock.
    /// </summary>
    /// \author Matt Bolt
    public class ConditionIds {
        private readonly string _prefix;
        private readonly AtomicLong _ids;

        public ConditionIds(string prefix, long startingId) {
            _prefix = prefix;
            _ids = new AtomicLong(startingId);
        }

        /// <summary>
        /// Creates and returns the next condition id.
        /// </summary>
        /// <remarks>This method is thread safe.</remarks>
        public string Next() {
            return _prefix + '-' + _ids.Increment();
        }
    }
}

