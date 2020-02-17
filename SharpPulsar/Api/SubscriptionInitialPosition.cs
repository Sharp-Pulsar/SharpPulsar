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
namespace SharpPulsar.Api
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

		private static readonly IList<SubscriptionInitialPosition> ValueList = new List<SubscriptionInitialPosition>();

		static SubscriptionInitialPosition()
		{
			ValueList.Add(Latest);
			ValueList.Add(Earliest);
		}

		public enum InnerEnum
		{
			Latest,
			Earliest
		}

		public readonly InnerEnum InnerEnumValue;
		private readonly string _nameValue;
		private readonly int _ordinalValue;
		private static int _nextOrdinal = 0;


		public int Value {get;}

		public SubscriptionInitialPosition(string name, InnerEnum innerEnum, int value)
		{
			this.Value = value;

			_nameValue = name;
			_ordinalValue = _nextOrdinal++;
			InnerEnumValue = innerEnum;
		}



		public static IList<SubscriptionInitialPosition> values()
		{
			return ValueList;
		}

		public int ordinal()
		{
			return _ordinalValue;
		}

		public override string ToString()
		{
			return _nameValue;
		}

		public static SubscriptionInitialPosition valueOf(string name)
		{
			foreach (SubscriptionInitialPosition enumInstance in SubscriptionInitialPosition.ValueList)
			{
				if (enumInstance._nameValue == name)
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException(name);
		}
	}

}