using SharpPulsar.Interfaces;
using System;

namespace SharpPulsar
{
    [Serializable]
	public abstract class MessageRouterBase : IMessageRouter
	{
		public abstract int ChoosePartition<T>(IMessage<T> msg, TopicMetadata metadata);
		public abstract int ChoosePartition<T>(IMessage<T> msg);

        protected internal readonly IHash Hash;

		internal MessageRouterBase(HashingScheme hashingScheme)
		{
			
		}
	}
}
