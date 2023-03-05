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
namespace SharpPulsar.Common.Entity
{
	/// <summary>
	/// When creating a consumer, if the subscription does not exist, a new subscription will be created. By default the
	/// subscription will be created at the end of the topic. See
	/// <seealso cref="subscriptionInitialPosition(SubscriptionInitialPosition)"/> to configure the initial position behavior.
	/// 
	/// </summary>
	public sealed class SubscriptionInitialPosition
	{
		/// <summary>
		/// The latest position which means the start consuming position will be the last message.
		/// </summary>
		public static readonly SubscriptionInitialPosition Latest = new SubscriptionInitialPosition("Latest", InnerEnum.Latest, 0);

		/// <summary>
		/// The earliest position which means the start consuming position will be the first message.
		/// </summary>
		public static readonly SubscriptionInitialPosition Earliest = new SubscriptionInitialPosition("Earliest", InnerEnum.Earliest, 1);

		private static readonly IList<SubscriptionInitialPosition> valueList = new List<SubscriptionInitialPosition>();

		static SubscriptionInitialPosition()
		{
			valueList.Add(Latest);
			valueList.Add(Earliest);
		}

		public enum InnerEnum
		{
			Latest,
			Earliest
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;


		private readonly int value;

		internal SubscriptionInitialPosition(string name, InnerEnum innerEnum, int value)
		{
			this.value = value;

			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		public int Value
		{
			get
			{
				return value;
			}
		}


		public static IList<SubscriptionInitialPosition> Values()
		{
			return valueList;
		}

		public int Ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static SubscriptionInitialPosition ValueOf(string name)
		{
			foreach (SubscriptionInitialPosition enumInstance in SubscriptionInitialPosition.valueList)
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