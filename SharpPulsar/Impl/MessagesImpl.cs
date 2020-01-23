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
	using Preconditions = com.google.common.@base.Preconditions;
	using NotThreadSafe = net.jcip.annotations.NotThreadSafe;
	using SharpPulsar.Api;
	using SharpPulsar.Api;


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @NotThreadSafe public class MessagesImpl<T> implements SharpPulsar.api.Messages<T>
	public class MessagesImpl<T> : Messages<T>
	{

		private IList<Message<T>> messageList;

		private readonly int maxNumberOfMessages;
		private readonly long maxSizeOfMessages;

		private int currentNumberOfMessages;
		private long currentSizeOfMessages;

		public MessagesImpl(int MaxNumberOfMessages, long MaxSizeOfMessages)
		{
			this.maxNumberOfMessages = MaxNumberOfMessages;
			this.maxSizeOfMessages = MaxSizeOfMessages;
			messageList = MaxNumberOfMessages > 0 ? new List<Message<T>>(MaxNumberOfMessages) : new List<Message<T>>();
		}

		public virtual bool CanAdd(Message<T> Message)
		{
			if (maxNumberOfMessages <= 0 && maxSizeOfMessages <= 0)
			{
				return true;
			}
			return (maxNumberOfMessages > 0 && currentNumberOfMessages + 1 <= maxNumberOfMessages) || (maxSizeOfMessages > 0 && currentSizeOfMessages + Message.Data.Length <= maxSizeOfMessages);
		}

		public virtual void Add(Message<T> Message)
		{
			if (Message == null)
			{
				return;
			}
			Preconditions.checkArgument(CanAdd(Message), "No more space to add messages.");
			currentNumberOfMessages++;
			currentSizeOfMessages += Message.Data.Length;
			messageList.Add(Message);
		}

		public override int Size()
		{
			return messageList.Count;
		}

		public virtual void Clear()
		{
			this.currentNumberOfMessages = 0;
			this.currentSizeOfMessages = 0;
			this.messageList.Clear();
		}

		public override IEnumerator<Message<T>> Iterator()
		{
			return messageList.GetEnumerator();
		}
	}

}