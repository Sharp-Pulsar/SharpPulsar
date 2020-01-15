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
namespace org.apache.pulsar.client.impl
{
	using Preconditions = com.google.common.@base.Preconditions;
	using NotThreadSafe = net.jcip.annotations.NotThreadSafe;
	using Message = org.apache.pulsar.client.api.Message;
	using Messages = org.apache.pulsar.client.api.Messages;


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @NotThreadSafe public class MessagesImpl<T> implements org.apache.pulsar.client.api.Messages<T>
	public class MessagesImpl<T> : Messages<T>
	{

		private IList<Message<T>> messageList;

		private readonly int maxNumberOfMessages;
		private readonly long maxSizeOfMessages;

		private int currentNumberOfMessages;
		private long currentSizeOfMessages;

		protected internal MessagesImpl(int maxNumberOfMessages, long maxSizeOfMessages)
		{
			this.maxNumberOfMessages = maxNumberOfMessages;
			this.maxSizeOfMessages = maxSizeOfMessages;
			messageList = maxNumberOfMessages > 0 ? new List<Message<T>>(maxNumberOfMessages) : new List<Message<T>>();
		}

		protected internal virtual bool canAdd(Message<T> message)
		{
			if (maxNumberOfMessages <= 0 && maxSizeOfMessages <= 0)
			{
				return true;
			}
			return (maxNumberOfMessages > 0 && currentNumberOfMessages + 1 <= maxNumberOfMessages) || (maxSizeOfMessages > 0 && currentSizeOfMessages + message.Data.length <= maxSizeOfMessages);
		}

		protected internal virtual void add(Message<T> message)
		{
			if (message == null)
			{
				return;
			}
			Preconditions.checkArgument(canAdd(message), "No more space to add messages.");
			currentNumberOfMessages++;
			currentSizeOfMessages += message.Data.length;
			messageList.Add(message);
		}

		public override int size()
		{
			return messageList.Count;
		}

		public virtual void clear()
		{
			this.currentNumberOfMessages = 0;
			this.currentSizeOfMessages = 0;
			this.messageList.Clear();
		}

		public override IEnumerator<Message<T>> iterator()
		{
			return messageList.GetEnumerator();
		}
	}

}