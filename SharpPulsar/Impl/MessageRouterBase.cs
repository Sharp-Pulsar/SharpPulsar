﻿using SharpPulsar.Api;

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
	using HashingScheme = SharpPulsar.Api.HashingScheme;
	using MessageRouter = SharpPulsar.Api.MessageRouter;

	[Serializable]
	public abstract class MessageRouterBase : MessageRouter
	{
		public abstract int choosePartition<T1>(Message<T1> Msg, TopicMetadata Metadata);
		public abstract int choosePartition<T1>(Message<T1> Msg);
		private const long SerialVersionUID = 1L;

		protected internal readonly Hash Hash;

		public MessageRouterBase(HashingScheme HashingScheme)
		{
			switch (HashingScheme)
			{
			case HashingScheme.JavaStringHash:
				this.Hash = JavaStringHash.Instance;
				break;
			case HashingScheme.Murmur3_32Hash:
			default:
				this.Hash = Murmur3_32Hash.Instance;
			break;
			}
		}
	}

}