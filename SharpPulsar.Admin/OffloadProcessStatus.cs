using Org.Apache.Pulsar.Client.Api;

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
namespace Org.Apache.Pulsar.Client.Admin
{
	using MessageId = Org.Apache.Pulsar.Client.Api.MessageId;
	using MessageIdImpl = Org.Apache.Pulsar.Client.Impl.MessageIdImpl;

	/// <summary>
	/// Status of offload process.
	/// </summary>
	public class OffloadProcessStatus : LongRunningProcessStatus
	{

		public MessageIdImpl FirstUnoffloadedMessage;

		public OffloadProcessStatus() : base(Status.NotRun, "")
		{
			FirstUnoffloadedMessage = (MessageIdImpl)MessageIdFields.Earliest;
		}

		private OffloadProcessStatus(Status Status, string LastError, MessageIdImpl FirstUnoffloadedMessage)
		{
			this.Status = Status;
			this.LastError = LastError;
			this.FirstUnoffloadedMessage = FirstUnoffloadedMessage;
		}

		public static OffloadProcessStatus ForStatus(Status Status)
		{
			return new OffloadProcessStatus(Status, "", (MessageIdImpl)MessageIdFields.Earliest);
		}

		public static OffloadProcessStatus ForError(string LastError)
		{
			return new OffloadProcessStatus(Status.ERROR, LastError, (MessageIdImpl)MessageIdFields.Earliest);
		}

		public static OffloadProcessStatus ForSuccess(MessageIdImpl MessageId)
		{
			return new OffloadProcessStatus(Status.SUCCESS, "", MessageId);
		}
	}

}