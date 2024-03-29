﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using SharpPulsar.Extension;
using SharpPulsar.Interfaces;

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
namespace SharpPulsar
{
    /// <summary>
    /// A MessageId implementation that contains a map of <partitionName, MessageId>.
    /// This is useful when MessageId is need for partition/multi-topics/pattern consumer.
    /// e.g. seek(), ackCumulative(), getLastMessageId().
    /// </summary>
    public class MultiMessageId : IMessageId
	{
		public readonly ImmutableDictionary<string, IMessageId> Map;

		public MultiMessageId(IDictionary<string, IMessageId> map)
		{
			Map = map?.ToImmutableDictionary();
		}

		// TODO: Add support for Serialization and Deserialization
		//  https://github.com/apache/pulsar/issues/4940
		public byte[] ToByteArray()
		{
			throw new NotImplementedException();
		}

		public override int GetHashCode()
		{
			return Map.GetHashCode();
		}

		// If all messageId in map are same Size, and all bigger/smaller than the other, return valid value.
		public int CompareTo(IMessageId o)
		{
			if (!(o is MultiMessageId))
			{
				throw new ArgumentException("expected MultiMessageId object. Got instance of " + o.GetType().FullName);
			}

			var other = (MultiMessageId) o;
			var otherMap = other.Map;

			if ((Map == null || Map.Count == 0) && (otherMap == null || otherMap.Count == 0))
			{
				return 0;
			}

			if (otherMap == null || Map == null || otherMap.Count != Map.Count)
			{
				throw new ArgumentException("Current Size and other Size not equals");
			}

			var result = 0;
			foreach (var entry in HashMapHelper.SetOfKeyValuePairs(Map))
			{
				if (!otherMap.TryGetValue(entry.Key, out var otherMessage))
				{
					throw new ArgumentException("Other MessageId not have topic " + entry.Key);
				}

				var currentResult = entry.Value.CompareTo(otherMessage);
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
					throw new ArgumentException("Different MessageId in Map get different compare result");
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
			if (!(obj is MultiMessageId))
			{
				throw new ArgumentException("expected MultiMessageId object. Got instance of " + obj.GetType().FullName);
			}

			var other = (MultiMessageId) obj;

			try
			{
				return CompareTo(other) == 0;
			}
			catch (ArgumentException)
			{
				return false;
			}
		}
	}

}