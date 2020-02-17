using System.Collections.Generic;

/// <summary>
///*****************************************************************************
/// Copyright 2014 Trevor Robinson
/// 
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
/// 
///   http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
/// *****************************************************************************
/// </summary>
namespace SharpPulsar.Protocol.Circe
{

	/// <summary>
	/// Flags indicating the support available for some set of hash algorithm.
	/// </summary>
	public sealed class HashSupport
	{
		/// <summary>
		/// Indicates that the hash algorithm is available in hardware-accelerated
		/// native code as an <seealso cref="IncrementalIntHash"/> or
		/// <seealso cref="IncrementalLongHash"/>, depending on which of <seealso cref="INT_SIZED"/> or
		/// <seealso cref="LONG_SIZED"/> is set.
		/// </summary>
		public static readonly HashSupport HardwareIncremental = new HashSupport("HardwareIncremental", InnerEnum.HardwareIncremental, 10);
		/// <summary>
		/// Indicates that the hash algorithm is available in hardware-accelerated
		/// native code.
		/// </summary>
		public static readonly HashSupport Hardware = new HashSupport("HARDWARE", InnerEnum.Hardware, 20);
		/// <summary>
		/// Indicates that the hash algorithm is available in native code as a
		/// <seealso cref="IncrementalIntHash"/> or <seealso cref="IncrementalLongHash"/>, depending on
		/// which of <seealso cref="INT_SIZED"/> or <seealso cref="LONG_SIZED"/> is set.
		/// </summary>
		public static readonly HashSupport NativeIncremental = new HashSupport("NativeIncremental", InnerEnum.NativeIncremental, 30);
		/// <summary>
		/// Indicates that the hash algorithm is available in native code.
		/// </summary>
		public static readonly HashSupport Native = new HashSupport("NATIVE", InnerEnum.Native, 40);
		/// <summary>
		/// Indicates that the incremental hash algorithm supports unsafe memory
		/// access via <seealso cref="IncrementalIntHash.resume(int, long, long)"/> or
		/// <seealso cref="IncrementalLongHash.resume(long, long, long)"/>, depending on which
		/// of <seealso cref="INT_SIZED"/> or <seealso cref="LONG_SIZED"/> is set.
		/// </summary>
		public static readonly HashSupport UnsafeIncremental = new HashSupport("UnsafeIncremental", InnerEnum.UnsafeIncremental, 50);
		/// <summary>
		/// Indicates that the stateful hash algorithm unsafe memory access via
		/// <seealso cref="StatefulHash.update(long, long)"/>. If <seealso cref="INT_SIZED"/> is also
		/// set, the function returned by <seealso cref="StatefulIntHash.asStateless()"/> also
		/// supports <seealso cref="StatelessIntHash.calculate(long, long)"/>. Similarly, if
		/// <seealso cref="LONG_SIZED"/> is also set, the function returned by
		/// <seealso cref="StatefulLongHash.asStateless()"/> also supports
		/// <seealso cref="StatelessLongHash.calculate(long, long)"/>.
		/// </summary>
		public static readonly HashSupport Unsafe = new HashSupport("UNSAFE", InnerEnum.Unsafe, 60);
		/// <summary>
		/// Indicates that the hash algorithm is available as a
		/// <seealso cref="IncrementalIntHash"/> or <seealso cref="IncrementalLongHash"/>, depending on
		/// which of <seealso cref="INT_SIZED"/> or <seealso cref="LONG_SIZED"/> is set.
		/// </summary>
		public static readonly HashSupport StatelessIncremental = new HashSupport("StatelessIncremental", InnerEnum.StatelessIncremental, 70);
		/// <summary>
		/// Indicates that the hash algorithm is available as an incremental stateful
		/// hash function, for which <seealso cref="StatefulHash.supportsIncremental()"/>
		/// returns {@code true}. This flag is implied by
		/// <seealso cref="STATELESS_INCREMENTAL"/>.
		/// </summary>
		public static readonly HashSupport Incremental = new HashSupport("INCREMENTAL", InnerEnum.Incremental, 80);
		/// <summary>
		/// Indicates that the hash algorithm is available as a
		/// <seealso cref="StatefulIntHash"/> and <seealso cref="StatelessIntHash"/>.
		/// </summary>
		public static readonly HashSupport IntSized = new HashSupport("IntSized", InnerEnum.IntSized, 90);
		/// <summary>
		/// Indicates that the hash algorithm is available as a
		/// <seealso cref="StatefulLongHash"/> and <seealso cref="StatelessLongHash"/>.
		/// </summary>
		public static readonly HashSupport LongSized = new HashSupport("LongSized", InnerEnum.LongSized, 90);
		/// <summary>
		/// Indicates that the hash algorithm is available as a <seealso cref="StatefulHash"/>.
		/// If this flag is not set, the algorithm is not supported at all.
		/// </summary>
		public static readonly HashSupport Stateful = new HashSupport("STATEFUL", InnerEnum.Stateful, 100);

		private static readonly IList<HashSupport> ValueList = new List<HashSupport>();

		static HashSupport()
		{
			ValueList.Add(HardwareIncremental);
			ValueList.Add(Hardware);
			ValueList.Add(NativeIncremental);
			ValueList.Add(Native);
			ValueList.Add(UnsafeIncremental);
			ValueList.Add(Unsafe);
			ValueList.Add(StatelessIncremental);
			ValueList.Add(Incremental);
			ValueList.Add(IntSized);
			ValueList.Add(LongSized);
			ValueList.Add(Stateful);
		}

		public enum InnerEnum
		{
			HardwareIncremental,
			Hardware,
			NativeIncremental,
			Native,
			UnsafeIncremental,
			Unsafe,
			StatelessIncremental,
			Incremental,
			IntSized,
			LongSized,
			Stateful
		}

		public readonly InnerEnum InnerEnumValue;
		private readonly string _nameValue;
		private readonly int _ordinalValue;
		private static int _nextOrdinal = 0;

		/// <summary>
		/// The minimum priority value, indicating the highest priority. All flags
		/// have a priority value greater than this.
		/// </summary>
		public const int MinPriority = 0;

		/// <summary>
		/// The maximum priority value, indicating the lowest priority. All flags
		/// have a priority value less than this.
		/// </summary>
		public const int MaxPriority = 110;

		public int Priority {get;}

		private HashSupport(string name, InnerEnum innerEnum, int priority)
		{
			Priority = priority;

			_nameValue = name;
			_ordinalValue = _nextOrdinal++;
			InnerEnumValue = innerEnum;
		}

		/// <summary>
		/// Returns the relative priority of a hash algorithm support flag, which is
		/// an indicator of its performance and flexibility. Lower values indicate
		/// higher priority.
		/// </summary>
		/// <returns> the priority of this flag (currently between 10 and 90) </returns>

		/// <summary>
		/// Returns the <seealso cref="getPriority() priority"/> of the highest-priority
		/// hash algorithm support flag in the given set of flags. If the set is
		/// empty, <seealso cref="MaxPriority"/> is returned.
		/// </summary>
		/// <param name="set"> a set of hash algorithm support flags </param>
		/// <returns> the highest priority (lowest value) in the set, or
		///         <seealso cref="MaxPriority"/> if empty </returns>
		public static int GetMaxPriority(ISet<HashSupport> set)
		{
			if (set.Count==0)
			{
				return MaxPriority;
			}
			return set.GetEnumerator().Current.Priority;
		}

		/// <summary>
		/// Compares the given sets of hash algorithm support flags for priority
		/// order. The set with the highest priority flag without a flag of matching
		/// priority in the other set has higher priority.
		/// </summary>
		/// <param name="set1"> the first set to be compared </param>
		/// <param name="set2"> the second set to be compared </param>
		/// <returns> a negative integer, zero, or a positive integer if the first set
		///         has priority higher than, equal to, or lower than the second </returns>
		public static int Compare(ISet<HashSupport> set1, ISet<HashSupport> set2)
		{
			// assumes iterators return flags in priority order
            using var i1 = set1.GetEnumerator();
            using var i2 = set2.GetEnumerator();
			var floor = MinPriority;
			while (i1.MoveNext() || i2.MoveNext())
			{
				int p1, p2;
				do
				{
					p1 = i1.MoveNext() ? i1.Current.Priority : MaxPriority;
				} while (p1 == floor);
				do
				{
					p2 = i2.MoveNext() ? i2.Current.Priority : MaxPriority;
				} while (p2 == floor);
				if (p1 < p2)
				{
					return -1;
				}
				if (p1 > p2)
				{
					return 1;
				}
				floor = p1;
			}
			return 0;
		}

	}

}