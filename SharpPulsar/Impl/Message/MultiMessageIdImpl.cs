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
namespace SharpPulsar.Impl.Message
{
	using Getter = lombok.Getter;
	using MessageId = org.apache.pulsar.client.api.MessageId;
	using NotImplementedException = sun.reflect.generics.reflectiveObjects.NotImplementedException;

	/// <summary>
	/// A MessageId implementation that contains a map of <partitionName, MessageId>.
	/// This is useful when MessageId is need for partition/multi-topics/pattern consumer.
	/// e.g. seek(), ackCumulative(), getLastMessageId().
	/// </summary>
	public class MultiMessageIdImpl : MessageId
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Getter private java.util.Map<String, org.apache.pulsar.client.api.MessageId> map;
		private IDictionary<string, MessageId> map;

		internal MultiMessageIdImpl(IDictionary<string, MessageId> map)
		{
			this.map = map;
		}

		// TODO: Add support for Serialization and Deserialization
		//  https://github.com/apache/pulsar/issues/4940
		public override sbyte[] toByteArray()
		{
			throw new NotImplementedException();
		}

		public override int GetHashCode()
		{
			return Objects.hash(map);
		}

		// If all messageId in map are same size, and all bigger/smaller than the other, return valid value.
		public override int compareTo(MessageId o)
		{
			if (!(o is MultiMessageIdImpl))
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				throw new System.ArgumentException("expected MultiMessageIdImpl object. Got instance of " + o.GetType().FullName);
			}

			MultiMessageIdImpl other = (MultiMessageIdImpl) o;
			IDictionary<string, MessageId> otherMap = other.Map;

			if ((map == null || map.Count == 0) && (otherMap == null || otherMap.Count == 0))
			{
				return 0;
			}

			if (otherMap == null || map == null || otherMap.Count != map.Count)
			{
				throw new System.ArgumentException("Current size and other size not equals");
			}

			int result = 0;
			foreach (KeyValuePair<string, MessageId> entry in map.SetOfKeyValuePairs())
			{
				MessageId otherMessage = otherMap[entry.Key];
				if (otherMessage == null)
				{
					throw new System.ArgumentException("Other MessageId not have topic " + entry.Key);
				}

				int currentResult = entry.Value.compareTo(otherMessage);
				if (result == 0)
				{
					result = currentResult;
				}
				else if (currentResult == 0)
				{
					continue;
				}
				else if (result != currentResult)
				{
					throw new System.ArgumentException("Different MessageId in Map get different compare result");
				}
				else
				{
					continue;
				}
			}

			return result;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is MultiMessageIdImpl))
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				throw new System.ArgumentException("expected MultiMessageIdImpl object. Got instance of " + obj.GetType().FullName);
			}

			MultiMessageIdImpl other = (MultiMessageIdImpl) obj;

			try
			{
				return compareTo(other) == 0;
			}
			catch (System.ArgumentException)
			{
				return false;
			}
		}
	}

}