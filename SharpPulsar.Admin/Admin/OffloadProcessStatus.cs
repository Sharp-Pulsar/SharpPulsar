﻿/// <summary>
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
namespace org.apache.pulsar.client.admin
{
	using MessageId = org.apache.pulsar.client.api.MessageId;
	using MessageIdImpl = org.apache.pulsar.client.impl.MessageIdImpl;

	/// <summary>
	/// Status of offload process.
	/// </summary>
	public class OffloadProcessStatus : LongRunningProcessStatus
	{

		public MessageIdImpl firstUnoffloadedMessage;

		public OffloadProcessStatus() : base(Status.NOT_RUN, "")
		{
			firstUnoffloadedMessage = (MessageIdImpl)MessageId.earliest;
		}

		private OffloadProcessStatus(Status status, string lastError, MessageIdImpl firstUnoffloadedMessage)
		{
			this.status = status;
			this.lastError = lastError;
			this.firstUnoffloadedMessage = firstUnoffloadedMessage;
		}

		public static OffloadProcessStatus forStatus(Status status)
		{
			return new OffloadProcessStatus(status, "", (MessageIdImpl)MessageId.earliest);
		}

		public static OffloadProcessStatus forError(string lastError)
		{
			return new OffloadProcessStatus(Status.ERROR, lastError, (MessageIdImpl)MessageId.earliest);
		}

		public static OffloadProcessStatus forSuccess(MessageIdImpl messageId)
		{
			return new OffloadProcessStatus(Status.SUCCESS, "", messageId);
		}
	}

}