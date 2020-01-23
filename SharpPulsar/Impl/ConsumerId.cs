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
	using Objects = com.google.common.@base.Objects;
	using ComparisonChain = com.google.common.collect.ComparisonChain;

	public class ConsumerId : IComparable<ConsumerId>
	{
		public virtual Topic {get;}
		public virtual Subscription {get;}

		public ConsumerId(string Topic, string Subscription)
		{
			this.Topic = Topic;
			this.Subscription = Subscription;
		}



		public override int GetHashCode()
		{
			return Objects.hashCode(Topic, Subscription);
		}

		public override bool Equals(object Obj)
		{
			if (Obj is ConsumerId)
			{
				ConsumerId Other = (ConsumerId) Obj;
				return Objects.equal(this.Topic, Other.Topic) && Objects.equal(this.Subscription, Other.Subscription);
			}

			return false;
		}

		public override int CompareTo(ConsumerId O)
		{
			return ComparisonChain.start().compare(this.Topic, O.Topic).compare(this.Subscription, O.Subscription).result();
		}

	}

}