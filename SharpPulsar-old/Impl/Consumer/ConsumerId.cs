using System;

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
namespace SharpPulsar.Impl
{

	public class ConsumerId : IComparable<ConsumerId>
	{
		private readonly string topic;
		private readonly string subscription;

		public ConsumerId(string topic, string subscription)
		{
			this.topic = topic;
			this.subscription = subscription;
		}

		public virtual string Topic
		{
			get
			{
				return topic;
			}
		}

		public virtual string Subscription
		{
			get
			{
				return subscription;
			}
		}

		public override int GetHashCode()
		{
			return Objects.hashCode(topic, subscription);
		}

		public override bool Equals(object obj)
		{
			if (obj is ConsumerId other)
			{
				return Equals(topic, other.topic) && Equals(subscription, other.subscription);
			}

			return false;
		}

		public virtual int CompareTo(ConsumerId o)
		{
			return ComparisonChain.start().compare(this.topic, o.topic).compare(this.subscription, o.subscription).result();
		}

	}

}