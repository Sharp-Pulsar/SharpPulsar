/// <summary>
/// Licensed to the Apache Software Foundation (ASF) under one
/// or more contributor license agreements.  See the NOTICE file
/// distributed with this work for additional information
/// regarding copyright ownership.  The ASF licenses this file
/// to you under the Apache License, Version 2.0 (the
/// "License"); you may not use this file except in compliance
/// with the License.  You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing,
/// software distributed under the License is distributed on an
/// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
/// KIND, either express or implied.  See the License for the
/// specific language governing permissions and limitations
/// under the License.
/// </summary>
namespace SharpPulsar.Shared
{
    /// <summary>
    /// Int range.
    /// </summary>
    [Serializable]
    public class Range : IComparable<Range>

    {

        private readonly int start;
		private readonly int end;


		public Range(int start, int end)
		{
			if (end < start)
			{
				throw new System.ArgumentException("Range end must >= range start.");
			}
			this.start = start;
			this.end = end;
		}

		public static Range Of(int start, int end)
		{
			return new Range(start, end);
		}

		public virtual int Start
		{
			get
			{
				return start;
			}
		}

		public virtual int End
		{
			get
			{
				return end;
			}
		}

		public virtual Range Intersect(Range range)
		{
			int start = range.Start > this.Start ? range.Start : this.Start;
			int end = range.End < End ? range.End : this.End;
			if (end >= start)
			{
				return Of(start, end);
			}
			else
			{
				return null;
			}
		}

		public override string ToString()
		{
			return "[" + start + ", " + end + "]";
		}
        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || this.GetType() != o.GetType())
            {
                return false;
            }
            Range range = (Range)o;
            return start == range.start && end == range.end;
        }

        /*public override int GetHashCode()
        {
            return Objects.hash(start, end);
        }*/
    
        public virtual int CompareTo(Range o)
        {
            int result = Start.CompareTo(o.start);
            if (result == 0)
            {
                result = End.CompareTo(o.end);
            }
            return result;
        }

        /// <summary>
        /// Check if the value is in the range. </summary>
        /// <param name="value"> </param>
        /// <returns> true if the value is in the range. </returns>
        public virtual bool Contains(int value)
        {
            return value >= start && value <= end;
        }

        /// <summary>
        /// Check if the range is fully contained in the other range. </summary>
        /// <param name="otherRange"> </param>
        /// <returns> true if the range is fully contained in the other range. </returns>
        public virtual bool Contains(Range otherRange)
        {
            return start <= otherRange.start && end >= otherRange.end;
        }

        /// <summary>
        /// Get the size of the range. </summary>
        /// <returns> the size of the range. </returns>
        public virtual int Size()
        {
            return end - start + 1;
        }

    }

}