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
namespace SharpPulsar.Tracker
{
	public class UnackMessageIdWrapper
	{
		internal IMessageId MessageId;
		internal int RedeliveryCount = 0;

		internal UnackMessageIdWrapper Create(IMessageId messageId, int redeliveryCount)
		{
			var unackMessageIdWrapper = new UnackMessageIdWrapper();
			unackMessageIdWrapper.MessageId = messageId;
			unackMessageIdWrapper.RedeliveryCount = redeliveryCount;
			return unackMessageIdWrapper;
		}


		internal UnackMessageIdWrapper ValueOf(IMessageId messageId)
		{
			return Create(messageId, 0);
		}

		internal UnackMessageIdWrapper ValueOf(IMessageId messageId, int redeliveryCount)
		{
			return Create(messageId, redeliveryCount);
		}

		public virtual void Recycle()
		{
			MessageId = null;
			RedeliveryCount = 0;
		}

		public override bool Equals(object Obj)
		{
			if (this == Obj)
			{
				return true;
			}

			if (Obj == null)
			{
				return false;
			}

			if (Obj is UnackMessageIdWrapper)
			{
                var Other = (UnackMessageIdWrapper) Obj;
				if (MessageId.Equals(Other.MessageId))
				{
					return true;
				}
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (MessageId == null ? 0 : MessageId.GetHashCode());
		}

		public override string ToString()
		{
			return "UnackMessageIdWrapper [messageId=" + MessageId + ", redeliveryCount=" + RedeliveryCount + "]";
		}

	}

}