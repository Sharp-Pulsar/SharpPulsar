using System.Collections.Generic;

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
namespace SharpPulsar.Admin.Model
{
	using InterfaceAudience = org.apache.pulsar.common.classification.InterfaceAudience;
	using InterfaceStability = org.apache.pulsar.common.classification.InterfaceStability;

// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @InterfaceAudience.Public @InterfaceStability.Stable public enum OffloadedReadPriority
	public sealed class OffloadedReadPriority
	{
		/// <summary>
		/// For offloaded messages, readers will try to read from bookkeeper at first,
		/// if messages not exist at bookkeeper then read from offloaded storage.
		/// </summary>
		public static readonly OffloadedReadPriority BookkeeperFirst = new OffloadedReadPriority("BookkeeperFirst", InnerEnum.BookkeeperFirst, "bookkeeper-first");
		/// <summary>
		/// For offloaded messages, readers will try to read from offloaded storage first,
		/// even they are still exist in bookkeeper.
		/// </summary>
		public static readonly OffloadedReadPriority TieredStorageFirst = new OffloadedReadPriority("TieredStorageFirst", InnerEnum.TieredStorageFirst, "tiered-storage-first");

		private static readonly List<OffloadedReadPriority> valueList = new List<OffloadedReadPriority>();

		static OffloadedReadPriority()
		{
			valueList.Add(BookkeeperFirst);
			valueList.Add(TieredStorageFirst);
		}

		public enum InnerEnum
		{
			BookkeeperFirst,
			TieredStorageFirst
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		internal Private readonly;

		internal OffloadedReadPriority(string name, InnerEnum innerEnum, string Value)
		{
			this.value = Value;

			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		public bool equalsName(string OtherName)
		{
			return value.Equals(OtherName);
		}

		public override string ToString()
		{
			return value;
		}

		public static OffloadedReadPriority fromString(string Str)
		{
			foreach (OffloadedReadPriority Value in OffloadedReadPriority.values())
			{
				if (Value.value.Equals(Str))
				{
					return Value;
				}
			}

// JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
// JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			throw new System.ArgumentException("--offloadedReadPriority parameter must be one of " + OffloadedReadPriority.values().Select(OffloadedReadPriority::toString).collect(Collectors.joining(",")) + " but got: " + Str);
		}

		public string Value
		{
			get
			{
				return value;
			}
		}

		public static OffloadedReadPriority[] values()
		{
			return valueList.ToArray();
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static OffloadedReadPriority valueOf(string name)
		{
			foreach (OffloadedReadPriority enumInstance in OffloadedReadPriority.valueList)
			{
				if (enumInstance.nameValue == name)
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException(name);
		}
	}
}