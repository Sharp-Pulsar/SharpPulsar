using System;
using System.Collections;
using System.Collections.Generic;
using SharpPulsar.Api;

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
	public class MessagesImpl<T> : IMessages<T>
	{

		private IList<IMessage<T>> _messageList;

		private readonly int _maxNumberOfMessages;
		private readonly long _maxSizeOfMessages;

		private int _currentNumberOfMessages;
		private long _currentSizeOfMessages;

		public MessagesImpl(int maxNumberOfMessages, long maxSizeOfMessages)
		{
			this._maxNumberOfMessages = maxNumberOfMessages;
			this._maxSizeOfMessages = maxSizeOfMessages;
			_messageList = maxNumberOfMessages > 0 ? new List<IMessage<T>>(maxNumberOfMessages) : new List<IMessage<T>>();
		}

		public virtual bool CanAdd(IMessage<T> message)
		{
			if (_maxNumberOfMessages <= 0 && _maxSizeOfMessages <= 0)
			{
				return true;
			}
			return (_maxNumberOfMessages > 0 && _currentNumberOfMessages + 1 <= _maxNumberOfMessages) || (_maxSizeOfMessages > 0 && _currentSizeOfMessages + message.Data.Length <= _maxSizeOfMessages);
		}

		public virtual void Add(IMessage<T> message)
		{
			if (message == null)
			{
				return;
			}
			if(!CanAdd(message))
                throw new ArgumentException("No more space to add messages.");
			_currentNumberOfMessages++;
			_currentSizeOfMessages += message.Data.Length;
			_messageList.Add(message);
		}

		public int Size()
		{
			return _messageList.Count;
		}

		public virtual void Clear()
		{
			this._currentNumberOfMessages = 0;
			this._currentSizeOfMessages = 0;
			this._messageList.Clear();
		}

		public IEnumerator<IMessage<T>> Iterator()
		{
			return _messageList.GetEnumerator();
		}

        public IEnumerator<IMessage<T>> GetEnumerator()
        {
           return Iterator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}