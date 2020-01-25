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
namespace SharpPulsar.Impl
{
	using Getter = lombok.Getter;
	using IMessageId = SharpPulsar.Api.IMessageId;
	using NotImplementedException = sun.reflect.generics.reflectiveObjects.NotImplementedException;

	/// <summary>
	/// A MessageId implementation that contains a map of <partitionName, MessageId>.
	/// This is useful when MessageId is need for partition/multi-topics/pattern consumer.
	/// e.g. seek(), ackCumulative(), getLastMessageId().
	/// </summary>
	[Serializable]
	public class MultiMessageIdImpl : IMessageId
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Getter private java.util.Map<String, SharpPulsar.api.MessageId> map;
		private IDictionary<string, IMessageId> map;

		public MultiMessageIdImpl(IDictionary<string, IMessageId> Map)
		{
			this.map = Map;
		}

		// TODO: Add support for Serialization and Deserialization
		//  https://github.com/apache/pulsar/issues/4940
		public override sbyte[] ToByteArray()
		{
			throw new NotImplementedException();
		}

		public override int GetHashCode()
		{
			return Objects.hash(map);
		}

		// If all messageId in map are same size, and all bigger/smaller than the other, return valid value.
		public override int CompareTo(IMessageId O)
		{
			if (!(O is MultiMessageIdImpl))
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				throw new System.ArgumentException("expected MultiMessageIdImpl object. Got instance of " + O.GetType().FullName);
			}

			MultiMessageIdImpl Other = (MultiMessageIdImpl) O;
			IDictionary<string, IMessageId> OtherMap = Other.Map;

			if ((map == null || map.Count == 0) && (OtherMap == null || OtherMap.Count == 0))
			{
				return 0;
			}

			if (OtherMap == null || map == null || OtherMap.Count != map.Count)
			{
				throw new System.ArgumentException("Current size and other size not equals");
			}

			int Result = 0;
			foreach (KeyValuePair<string, IMessageId> Entry in map.SetOfKeyValuePairs())
			{
				IMessageId OtherMessage = OtherMap[Entry.Key];
				if (OtherMessage == null)
				{
					throw new System.ArgumentException("Other MessageId not have topic " + Entry.Key);
				}

				int CurrentResult = Entry.Value.compareTo(OtherMessage);
				if (Result == 0)
				{
					Result = CurrentResult;
				}
				else if (CurrentResult == 0)
				{
					continue;
				}
				else if (Result != CurrentResult)
				{
					throw new System.ArgumentException("Different MessageId in Map get different compare result");
				}
				else
				{
					continue;
				}
			}

			return Result;
		}

		public override bool Equals(object Obj)
		{
			if (!(Obj is MultiMessageIdImpl))
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				throw new System.ArgumentException("expected MultiMessageIdImpl object. Got instance of " + Obj.GetType().FullName);
			}

			MultiMessageIdImpl Other = (MultiMessageIdImpl) Obj;

			try
			{
				return CompareTo(Other) == 0;
			}
			catch (System.ArgumentException)
			{
				return false;
			}
		}
	}

}