using System;
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
namespace Org.Apache.Pulsar.Common.Policies.Data
{
	/// <summary>
	/// Topic types -- partitioned or non-partitioned.
	/// </summary>
	public sealed class TopicType
	{
		public static readonly TopicType PARTITIONED = new TopicType("PARTITIONED", InnerEnum.PARTITIONED, "partitioned");
		public static readonly TopicType NonPartitioned = new TopicType("NonPartitioned", InnerEnum.NonPartitioned, "non-partitioned");

		private static readonly List<TopicType> valueList = new List<TopicType>();

		static TopicType()
		{
			valueList.Add(PARTITIONED);
			valueList.Add(NonPartitioned);
		}

		public enum InnerEnum
		{
			PARTITIONED,
			NonPartitioned
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;
		internal Private String;

		internal TopicType(string name, InnerEnum innerEnum, string Type)
		{
			this.type = Type;

			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		public override string ToString()
		{
			return type;
		}

		public static bool isValidTopicType(string Type)
		{
			foreach (TopicType TopicType in TopicType.values())
			{
				if (TopicType.ToString().Equals(Type, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		public static TopicType[] values()
		{
			return valueList.ToArray();
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static TopicType valueOf(string name)
		{
			foreach (TopicType enumInstance in TopicType.valueList)
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