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
namespace SharpPulsar.Admin.Model
{
	/// <summary>
	/// The policy type of auto failover.
	/// </summary>
	public sealed class AutoFailoverPolicyType
	{
		public static readonly AutoFailoverPolicyType MinAvailable = new AutoFailoverPolicyType("MinAvailable", InnerEnum.MinAvailable);

		private static readonly List<AutoFailoverPolicyType> valueList = new List<AutoFailoverPolicyType>();

		static AutoFailoverPolicyType()
		{
			valueList.Add(MinAvailable);
		}

		public enum InnerEnum
		{
			MinAvailable
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		private AutoFailoverPolicyType(string name, InnerEnum innerEnum)
		{
			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		public static AutoFailoverPolicyType fromString(string AutoFailoverPolicyTypeName)
		{
			foreach (AutoFailoverPolicyType AutoFailoverPolicyType in AutoFailoverPolicyType.values())
			{
				if (AutoFailoverPolicyType.ToString().Equals(AutoFailoverPolicyTypeName, StringComparison.OrdinalIgnoreCase))
				{
					return AutoFailoverPolicyType;
				}
			}
			return null;
		}

		public static AutoFailoverPolicyType[] values()
		{
			return valueList.ToArray();
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static AutoFailoverPolicyType valueOf(string name)
		{
			foreach (AutoFailoverPolicyType enumInstance in AutoFailoverPolicyType.valueList)
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