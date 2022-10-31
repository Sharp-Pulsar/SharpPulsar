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
namespace Org.Apache.Pulsar.Client.Impl
{
	using Recycler = io.netty.util.Recycler;
	using Getter = lombok.Getter;
	using MessageId = Org.Apache.Pulsar.Client.Api.MessageId;

// JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
// ORIGINAL LINE: @Getter public class UnackMessageIdWrapper
	public class UnackMessageIdWrapper
	{

		private static readonly Recycler<UnackMessageIdWrapper> rECYCLER = new RecyclerAnonymousInnerClass();

		private class RecyclerAnonymousInnerClass : Recycler<UnackMessageIdWrapper>
		{
			protected internal override UnackMessageIdWrapper newObject(Handle<UnackMessageIdWrapper> Handle)
			{
				return new UnackMessageIdWrapper(Handle);
			}
		}

		private readonly Recycler.Handle<UnackMessageIdWrapper> recyclerHandle;
		private MessageId messageId;
		private int redeliveryCount = 0;

		private UnackMessageIdWrapper(Recycler.Handle<UnackMessageIdWrapper> RecyclerHandle)
		{
			this.recyclerHandle = RecyclerHandle;
		}

		private static UnackMessageIdWrapper Create(MessageId MessageId, int RedeliveryCount)
		{
			UnackMessageIdWrapper UnackMessageIdWrapper = rECYCLER.get();
			UnackMessageIdWrapper.messageId = MessageId;
			UnackMessageIdWrapper.redeliveryCount = RedeliveryCount;
			return UnackMessageIdWrapper;
		}


		public static UnackMessageIdWrapper ValueOf(MessageId MessageId)
		{
			return Create(MessageId, 0);
		}

		public static UnackMessageIdWrapper ValueOf(MessageId MessageId, int RedeliveryCount)
		{
			return Create(MessageId, RedeliveryCount);
		}

		public virtual void Recycle()
		{
			messageId = null;
			redeliveryCount = 0;
			recyclerHandle.recycle(this);
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
				UnackMessageIdWrapper Other = (UnackMessageIdWrapper) Obj;
				if (this.messageId.Equals(Other.messageId))
				{
					return true;
				}
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (messageId == null ? 0 : messageId.GetHashCode());
		}

		public override string ToString()
		{
			return "UnackMessageIdWrapper [messageId=" + messageId + ", redeliveryCount=" + redeliveryCount + "]";
		}

	}

}