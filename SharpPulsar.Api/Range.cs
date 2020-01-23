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
namespace SharpPulsar.Api
{
	/// <summary>
	/// Int range.
	/// </summary>
	public class Range
	{

		public virtual Start {get;}
		public virtual End {get;}


		public Range(int Start, int End)
		{
			if (End < Start)
			{
				throw new System.ArgumentException("Range end must >= range start.");
			}
			this.Start = Start;
			this.End = End;
		}

		public static Range Of(int Start, int End)
		{
			return new Range(Start, End);
		}



		public virtual Range Intersect(Range Range)
		{
			int Start = Range.Start > this.Start ? Range.Start : this.Start;
			int End = Range.End < this.End ? Range.End : this.End;
			if (End >= Start)
			{
				return Range.Of(Start, End);
			}
			else
			{
				return null;
			}
		}

		public override string ToString()
		{
			return "[" + Start + ", " + End + "]";
		}
	}

}