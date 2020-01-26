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
namespace SharpPulsar.Common.Naming
{
	/// <summary>
	/// Enumeration showing if a topic is persistent.
	/// </summary>
	public sealed class TopicDomain
	{
		public static readonly TopicDomain persistent = new TopicDomain("persistent", InnerEnum.persistent, "persistent");
		public static readonly TopicDomain non_persistent = new TopicDomain("non_persistent", InnerEnum.non_persistent, "non-persistent");

		private static readonly IList<TopicDomain> valueList = new List<TopicDomain>();

		static TopicDomain()
		{
			valueList.Add(persistent);
			valueList.Add(non_persistent);
		}

		public enum InnerEnum
		{
			persistent,
			non_persistent
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private string value_Conflict;

		private TopicDomain(string name, InnerEnum innerEnum, string value)
		{
			this.value_Conflict = value;

			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		public string value()
		{
			return this.value_Conflict;
		}

		public static TopicDomain getEnum(string value)
		{
			foreach (TopicDomain e in values())
			{
				if (e.value_Conflict.Equals(value, StringComparison.OrdinalIgnoreCase))
				{
					return e;
				}
			}
			throw new System.ArgumentException("Invalid topic domain: '" + value + "'");
		}

		public override string ToString()
		{
			return this.value_Conflict;
		}

		public static IList<TopicDomain> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public static TopicDomain valueOf(string name)
		{
			foreach (TopicDomain enumInstance in TopicDomain.valueList)
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